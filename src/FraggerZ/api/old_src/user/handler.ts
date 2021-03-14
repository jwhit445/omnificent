import { Handler, Context } from 'aws-lambda';
import { DynamoDB } from 'aws-sdk'
import { MatchStatus } from '../match/match';
import { getAllMatchesForUser } from '../match/handler';
import { get_all_from_ddb, get_user_from_ddb, update_user_from_ddb, User, user_to_ddb, user_to_ddb_update_params } from "./user";
import { UserStats } from './userStats';

export const INITIAL_MMR: number = 25;
export const INITIAL_SIGMA: number = 8.333;

const dynamoDb = new DynamoDB.DocumentClient()

export const register: Handler = (event: any, context: Context, callback: any) => {
    const data = JSON.parse(event.body);
    try {
        if (!validateUser(data)) {
            callback(new Error("Invalid request data"));
            return;
        }
    } catch (error) {
        callback(new Error("Invalid request data"));
        return;
    }
    data.RoCoMMR = INITIAL_MMR;
    data.RoCoSigma = INITIAL_SIGMA;
    const params = {
        TableName: process.env.DYNAMODB_TABLE,
        Item: user_to_ddb(data),
    }

    // write the user to the database
    dynamoDb.put(params, (error, result) => {
        // handle potential errors
        if (error) {
            callback(new Error('Couldn\'t create the user for id:' + params.Item.Id));
            return;
        }

        // create a response
        const response = {
            statusCode: 200,
            body: JSON.stringify(params.Item)
        }
        callback(null, response);
    })
};

function validateUser(user: User): boolean {
    try {
        return user.Username !== undefined && user.Username !== null
            && user.Id !== undefined && user.Id !== null
            && user.RoCoMMR !== undefined && user.RoCoMMR !== null
            && user.IronSightMMR !== undefined && user.IronSightMMR !== null
            && user.CrossFireMMR !== undefined && user.CrossFireMMR !== null;
    } catch (error) {
        return false;
    }
}


export const getOne = async (event: any, context: Context): Promise<any> => {
    try {
        // fetch user from the database by id
        const result = await get_user_from_ddb(dynamoDb, event.pathParameters.id);
        const response = {
            statusCode: 200,
            body: JSON.stringify(result),
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t fetch the user.',
        }
    }
};

export const update = async (event: any, context: Context): Promise<any> => {
    const data = JSON.parse(event.body)
    try {
        if (!validateUser(data)) {
            throw new Error("Invalid request data");
        }
    } catch (error) {
        throw new Error("Invalid request data");
    }

    try {
        // write the match changes to the database
        await update_user_from_ddb(dynamoDb, event.pathParameters.id, data);
        const response = {
            statusCode: 200,
            body: "User updated successfully"
        }
        return response;
    } catch (error) {
        throw new Error('Couldn\'t update the user for id:' + data.Id);
    }
};

export const getStats = async (event: any, context: Context): Promise<any> => {
    try {
        // fetch user from the database by id
        const result = await get_user_from_ddb(dynamoDb, event.pathParameters.id);
        const userStats: UserStats = new UserStats();
        userStats.Id = result.Id;
        userStats.Wins = 0;
        userStats.Losses = 0;
        userStats.RoCoMMR = result.RoCoMMR;
        const allMatches = await getAllMatchesForUser(userStats.Id, MatchStatus.Reported);
        if(allMatches === undefined || allMatches === null) {
            throw new Error('Error retrieving matches from db');
        }
        for (const matchCur of allMatches.Items) {
            if(matchCur.MatchNumber < 2089) { // 2088 was the last match in season 2
                continue;
            }
            if(new Date(matchCur.DateTimeEnded) < new Date(result.DateTimeLastStatReset)) {
                continue;
            }
            if(matchCur.Team1Ids.includes(userStats.Id)) {
                if(matchCur.WinningTeam === 1) {
                    userStats.Wins+=1;
                }
                else if(matchCur.WinningTeam === 2) {
                    userStats.Losses+=1;
                }
            }
            if(matchCur.Team2Ids.includes(userStats.Id)) {
                if(matchCur.WinningTeam === 1) {
                    userStats.Losses+=1;
                }
                else if(matchCur.WinningTeam === 2) {
                    userStats.Wins+=1;
                }
            }
        }
        if(result.PlacementMatchIds.length < 10) {
            userStats.RankPosition = -1;
        }
        else {
            const users = await getAllUsersByTopMmr();
            let idx: number = 0;
            let rankStanding: number = 0;
            while(users[idx].RoCoMMR > userStats.RoCoMMR) {
                if(users[idx].PlacementMatchIds.length >= 10) {
                    rankStanding+=1;
                }
                idx+=1;
            }
            userStats.RankPosition = rankStanding;
        }
        const response = {
            statusCode: 200,
            body: JSON.stringify(userStats),
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t fetch the user.\r\nError: '+ error,
        }
    }
};

export const getStatsWithMatches = async (event: any, context: Context): Promise<any> => {
    try {
        // fetch user from the database by id
        const result = await get_user_from_ddb(dynamoDb, event.pathParameters.id);
        const userStats: UserStats = new UserStats();
        userStats.Id = result.Id;
        userStats.Wins = 0;
        userStats.Losses = 0;
        userStats.RoCoMMR = result.RoCoMMR;
        const matchIds: any[] = [];
        const allMatches = await getAllMatchesForUser(userStats.Id, MatchStatus.Reported);
        const cancelledMatches = await getAllMatchesForUser(userStats.Id, MatchStatus.Cancelled);
        if(allMatches === undefined || allMatches === null) {
            throw new Error('Error retrieving matches from db');
        }
        for (const matchCur of allMatches.Items) {
            if(matchCur.MatchNumber < 112) { // The first MMR game. Some games before then have been incorrectly marked as reported.
                continue;
            }
            if(matchCur.Team1Ids.includes(userStats.Id)) {
                matchIds.push({ id: matchCur.Id, matchnumber: matchCur.MatchNumber, outcome: (matchCur.WinningTeam === 1 ? "Win" : "Loss") });
                if(matchCur.WinningTeam === 1) {
                    userStats.Wins+=1;
                }
                else if(matchCur.WinningTeam === 2) {
                    userStats.Losses+=1;
                }
            }
            if(matchCur.Team2Ids.includes(userStats.Id)) {
                matchIds.push({ id: matchCur.Id, matchnumber: matchCur.MatchNumber, outcome: (matchCur.WinningTeam === 2 ? "Win" : "Loss") });
                if(matchCur.WinningTeam === 1) {
                    userStats.Losses+=1;
                }
                else if(matchCur.WinningTeam === 2) {
                    userStats.Wins+=1;
                }
            }
        }
        for (const matchCur of cancelledMatches.Items) {
            if(matchCur.MatchNumber < 112) { // The first MMR game. Some games before then have been incorrectly marked as reported.
                continue;
            }
            if(matchCur.Team1Ids.includes(userStats.Id)) {
                matchIds.push({ id: matchCur.Id, matchnumber: matchCur.MatchNumber, outcome: "Cancelled" });
            }
            if(matchCur.Team2Ids.includes(userStats.Id)) {
                matchIds.push({ id: matchCur.Id, matchnumber: matchCur.MatchNumber, outcome: "Cancelled" });
            }
        }
        matchIds.sort((a, b) => (Number.parseInt(a.MatchNumber, 10) > Number.parseInt(b.MatchNumber, 10)) ? 1 : -1);
        const users = await getAllUsersByTopMmr();
        let rankStanding: number = 0;
        while(users[rankStanding].RoCoMMR > userStats.RoCoMMR) {
            rankStanding+=1;
        }
        userStats.RankPosition = rankStanding;
        const response = {
            statusCode: 200,
            body: JSON.stringify(matchIds)
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t fetch the user.\r\nError: '+ error,
        }
    }
};

export const getAll = async (event: any, context: Context): Promise<any> => {
    try {
        const filteredResults = await get_all_from_ddb(dynamoDb);
        const response = {
            statusCode: 200,
            body: JSON.stringify(filteredResults),
        };
        return response;
    } catch (error) {
        throw new Error('Couldn\'t fetch all users: '+error);
    }
};

export const getAllUsersByTopMmr = async (): Promise<any> => {
    try {
        const filteredResults = await get_all_from_ddb(dynamoDb);
        filteredResults.sort((a: any, b: any) => (a.RoCoMMR > b.RoCoMMR) ? -1 : 1);
        return filteredResults;
    } catch (error) {
        throw new Error('Couldn\'t find any users in the database: ' + error);
    }
}

export const getLeaderboard = async (event: any, context: Context): Promise<any> => {
    try {
        const result = await getAllUsersByTopMmr();
        const response = {
            statusCode: 200,
            body: JSON.stringify(result.slice(0, 20)),
        };
        return response;
    } catch (error) {
        return {
          statusCode: error.statusCode || 501,
          headers: { 'Content-Type': 'text/plain' },
          body: 'Couldn\'t fetch users for the leaderboard: \n' + error,
        };
    }
};