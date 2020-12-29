import { Handler, Context } from 'aws-lambda';
import { DynamoDB } from 'aws-sdk'
import { report, reportMatch } from '../match/handler';
import { get_all_matches_from_ddb, MatchStatus, match_to_ddb_update_params } from '../match/match';
import { get_all_from_ddb, get_user_from_ddb, update_user_from_ddb } from '../user/user';

const dynamoDb = new DynamoDB.DocumentClient()

export const resetAllMmr = async (event: any, context: Context): Promise<any> => {
    const data = JSON.parse(event.body)
    const newMmr = data.mmr;
    const newSigma = data.sigma;
    const tableName = data.tableName;
    try {
        await setMmrTo(newMmr, newSigma, tableName);
        return {
            statusCode: 200,
            body: "All users now have " + newMmr + " mmr and " + newSigma + " sigma"
        }
    } catch (error) {
        throw new Error('Couldn\'t reset one or more user\'s MMR: '+error);
    }
};

const setMmrTo = async (newMmr: number, newSigma: number, tableName: string | undefined): Promise<any> => {
    var filteredResults;
    try {
         filteredResults = await get_all_from_ddb(dynamoDb, tableName);
    } catch (error) {
        throw new Error('Couldn\'t fetch all users: '+error);
    }
    try {
        for (let i = 0; i < filteredResults.length; i++) {
            const userCur = filteredResults[i];
            userCur.RoCoMMR = newMmr;
            userCur.RoCoSigma = newSigma;
            userCur.PlacementMatchIds = [];
            userCur.WinStreakMatchIds = [];
            await update_user_from_ddb(dynamoDb,userCur.Id, userCur, tableName);
        }
    } catch (error) {
        throw new Error('Couldn\'t reset one or more user\'s MMR: '+error);
    }
}

export const redoAllMmr = async (event: any, context: Context): Promise<any> => {
    const data = JSON.parse(event.body)

    // const data = JSON.parse(event.body)
    const newMmr = data.mmr;
    const newSigma = data.sigma;
    const tableName = data.tableName;

    try {
        await setMmrTo(newMmr, newSigma,tableName);
    } catch (error) {
        return {
            statusCode: 500,
            body: 'Couldn\'t reset one or more user\'s MMR: '+error
        }
    }
    const startingMatchId = 112;
    var lastMatchReported: number = 112;
    try {
        var matches = await get_all_matches_from_ddb(dynamoDb, tableName);
        for (let i = 0; i < matches.length; i++) {
            const matchCur = matches[i];
            if(matchCur.MatchNumber < startingMatchId) {
                continue;
            }
            if(lastMatchReported - startingMatchId >= 100) {
                break;
            }
            if(matchCur.MatchStatus == MatchStatus.Reported) {
                await reportMatch(matchCur,matchCur.Id,tableName);
                lastMatchReported = matchCur.MatchNumber;
            }
        }
        return {
            statusCode: 200,
            body: "All users have had their mmr recalculated. Last match: " + lastMatchReported
        }
    } catch (error) {
        return {
            statusCode: 500,
            body: 'Couldn\'t update a user\'s mmr for a match. Last match reported: ' + lastMatchReported + "\n" + error
        }
    }
}