import { Handler, Context } from 'aws-lambda';
import { DynamoDB } from 'aws-sdk'
import { report, reportMatch } from '../match/handler';
import { get_all_matches_from_ddb, MatchStatus, match_to_ddb_update_params } from '../match/match';
import { get_all_non_reset_from_ddb, update_user_from_ddb } from '../user/user';

const dynamoDb = new DynamoDB.DocumentClient()

export const resetAllMmr = async (event: any, context: Context): Promise<any> => {
    const data = JSON.parse(event.body)
    const newMmr = data.mmr;
    const newSigma = data.sigma;
    try {
        await setMmrTo(newMmr, newSigma);
        return {
            statusCode: 200,
            body: "All users now have " + newMmr + " mmr and " + newSigma + " sigma"
        }
    } catch (error) {
        throw new Error('Couldn\'t reset one or more user\'s MMR: '+error);
    }
};

const setMmrTo = async (newMmr: number, newSigma: number): Promise<any> => {
    let filteredResults;
    try {
         filteredResults = await get_all_non_reset_from_ddb(dynamoDb, newMmr, newSigma);
    } catch (error) {
        throw new Error('Couldn\'t fetch all users: '+error);
    }
    try {
        for (const userCur of filteredResults) {
            userCur.RoCoMMR = newMmr;
            userCur.RoCoSigma = newSigma;
            userCur.PlacementMatchIds = [];
            userCur.WinStreakMatchIds = [];
            await update_user_from_ddb(dynamoDb,userCur.Id, userCur);
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
    try {
        await setMmrTo(newMmr, newSigma);
    } catch (error) {
        return {
            statusCode: 500,
            body: 'Couldn\'t reset one or more user\'s MMR: '+error
        }
    }
    const startingMatchId = 112;
    let lastMatchReported: number = 112;
    try {
        const matches = await get_all_matches_from_ddb(dynamoDb);
        for (const matchCur of matches) {
            if(matchCur.MatchNumber < startingMatchId) {
                continue;
            }
            if(lastMatchReported - startingMatchId >= 100) {
                break;
            }
            if(matchCur.MatchStatus === MatchStatus.Reported) {
                await reportMatch(matchCur,matchCur.Id);
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

export const addMigrateAttribute = async (event: any, context: Context) => {
    try {
        const params: any = {
            TableName: 'fraggerz-api-alpha',
            IndexName: 'InverseKey',
            KeyConditionExpression: 'EntityType = :hashKey',
            FilterExpression: "attribute_not_exists(Migrated)",
            ExpressionAttributeValues: {
                ':hashKey': 'user'
            },
            ExclusiveStartKey: undefined,
        };
        do {
            const result = await dynamoDb.query(params).promise();
            if(result.Items) {
                //Add Migrated=0 to row for each user item
                for(const item of result.Items) {
                    await dynamoDb.update({
                        TableName: 'fraggerz-api-alpha',
                        Key: {
                            Id: `${item.Id}`,
                            EntityType: `user`
                        },
                        UpdateExpression: 'set Migrated = :migrated',
                        ExpressionAttributeValues: {
                            ':migrated': '0'
                        }
                    }).promise();
                }
                
            }
            params.ExclusiveStartKey = result.LastEvaluatedKey;
        }
        while((params.ExclusiveStartKey !== undefined && params.ExclusiveStartKey !== null));
    } catch (error) {
        throw new Error('Couldn\'t fetch all non-reset users: '+error);
    }
};

export const migrateTableStream = async (event: any, context: Context) => {
    //console.log('Received event:', JSON.stringify(event, null, 2));
    for (const record of event.Records) {
        console.log(record.eventID);
        console.log(record.eventName);
        console.log('DynamoDB Record: %j', record.dynamodb);
    }
    return `Successfully processed ${event.Records.length} records.`;
};