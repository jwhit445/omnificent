import { Handler, Context } from 'aws-lambda';
import { DynamoDB } from 'aws-sdk'
import { rate, Rating, quality } from 'ts-trueskill';
import { get_all_matches_from_ddb, Match, MatchStatus, match_to_ddb, match_to_ddb_update_params } from './match';
import { v4 } from 'uuid';
import { get_user_from_ddb, update_user_from_ddb, User, user_to_ddb_update_params } from '../user/user';
import { INITIAL_SIGMA } from '../user/handler';

const dynamoDb = new DynamoDB.DocumentClient();

module.exports.register = async (event: any, context: Context): Promise<any> => {
    const data = JSON.parse(event.body)
    try {
        if (!validateMatch(data)) {
            throw new Error("Invalid request data");
        }
    } catch (error) {
        throw new Error("Invalid request data");
    }

    const params = {
        TableName: process.env.DYNAMODB_TABLE,
        Item: match_to_ddb(v4(), await getNextMatchNumber(), data),
    }

    // write the match to the database
    try {
        await dynamoDb.put(params).promise();
        // create a response
        const response = {
            statusCode: 200,
            body: JSON.stringify(params.Item)
        }
        return response;
    } catch (error) {
        throw new Error('Couldn\'t create the match for id:' + params.Item.Id);
    }
};

async function getNextMatchNumber(): Promise<number> {
    const result = await getAllMatches();
        if(result.Count && result.Items && result.Count > 0) {
            return result.Items[0].MatchNumber + 1;
        }
        else {
            // No record in the DB. Start with 1.
            return 1;
        }
}

export const getAllMatches = async (): Promise<any> => {
    return await dynamoDb.query({
        TableName: process.env.DYNAMODB_TABLE,
        IndexName: 'EntityTypeMatchNumberIndex',
        KeyConditionExpression: 'EntityType = :hashKey',
        ExpressionAttributeValues: {
          ':hashKey': 'match',
        },
        ScanIndexForward: false // true or false to sort by "date" Sort/Range key ascending or descending
    })
    .promise();
}

export const getAllMatchesForUser = async (userId: string, matchStatus: MatchStatus): Promise<any> => {
    return await dynamoDb.query({
        TableName: process.env.DYNAMODB_TABLE,
        IndexName: 'EntityTypeMatchNumberIndex',
        KeyConditionExpression: 'EntityType = :hashKey',
        FilterExpression: "MatchStatus = :mstatus AND (contains (Team1Ids, :userId) OR contains (Team2Ids, :userId))",
        ExpressionAttributeValues: {
          ':hashKey': 'match',
          ':userId': userId,
          ':mstatus': matchStatus,
        },
        ScanIndexForward: false // true or false to sort by "date" Sort/Range key ascending or descending
    })
    .promise();
}

function validateMatch(match: Match): boolean {
    try {
        return match.MapName !== null
        && match.MapName !== undefined && match.MatchNumber > -1
        && match.MatchStatus !== undefined && match.MatchStatus !== null
        && match.MatchType !== undefined && match.MatchType !== null;
    } catch (error) {
        return false;
    }
}

export const update = (event: any, context: Context, callback: any): Handler => {
    const data = JSON.parse(event.body)
    try {
        if (!validateMatch(data)) {
            callback(new Error("Invalid request data"));
            return;
        }
    } catch(error) {
        callback(new Error("Invalid request data"));
        return;
    }

    const params = match_to_ddb_update_params(event.pathParameters.id, data);

    // write the match changes to the database
    dynamoDb.update(params, (error, result) => {
        // handle potential errors
        if (error) {
            callback(new Error('Couldn\'t update the match for id:' + data.Id + "Error: " + error));
            return;
        }

        // create a response
        const response = {
            statusCode: 200,
            body: "Match updated successfully"
        }
        callback(null, response);
    })
}

export const report = async (event: any, context: Context): Promise<any> => {
    const data = JSON.parse(event.body)
    try {
        if (!validateMatch(data)) {
            throw new Error("Invalid request data");
        }
    } catch(error) {
        throw new Error("Invalid request data");
    }
    const match = data as Match;
    if(match.MatchStatus === MatchStatus.Reported) {
        throw new Error("This match has already been reported");
    }
    if(match.MatchStatus === MatchStatus.Cancelled) {
        throw new Error("This match was cancelled!");
    }
    try {
        await reportMatch(match, event.pathParameters.id);
        const response = {
            statusCode: 200,
            body: "Match reported successfully"
        }
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t report the match. \n' + error,
        };
    }
}

export const reportMatch = async (match: Match, id: string): Promise<any> => {
    match.MatchStatus = MatchStatus.Reported;
    // Get all users and their current mmr
    const team1Users: User[] = [];
    const team2Users: User[] = [];
    // For each user, call an "assign points" method that takes in:
    // 1. The user id and
    // 2. If they won or lost
    // 3. All of the opposing team's
    for (const userId of match.Team1Ids) {
        // Get the user from their id
        const userCur: User = await get_user_from_ddb(dynamoDb, userId);
        team1Users.push(userCur);
    }
    for (const userId of match.Team2Ids) {
        // Get the user from their id
        const userCur: User = await get_user_from_ddb(dynamoDb, userId);
        team2Users.push(userCur);
    }
    if(match.WinningTeam === 1) {
        const [updatedTeam1, updatedTeam2] = calculatePoints(team1Users, team2Users);
        await updateTeamUsers(match, updatedTeam1, updatedTeam2);
    }
    else if(match.WinningTeam === 2) {
        const [updatedTeam2, updatedTeam1] = calculatePoints(team2Users, team1Users);
        await updateTeamUsers(match, updatedTeam1, updatedTeam2);
    }
    else {
        throw new Error('Invalid winning team.');
    }
    const params = match_to_ddb_update_params(id, match);
    // write the match changes to the database
    await dynamoDb.update(params).promise();
}

const updateTeamUsers = async (match: Match, updatedTeam1: User[], updatedTeam2: User[]): Promise<any> => {
    for (const userCur of updatedTeam1) {
        if(userCur.PlacementMatchIds === undefined || userCur.PlacementMatchIds === null) {
            userCur.PlacementMatchIds = [];
        }
        if(userCur.PlacementMatchIds.length < 10) {
            userCur.PlacementMatchIds.push(match.Id);
        }
        await update_user_from_ddb(dynamoDb, userCur.Id, userCur);
    }
    for (const userCur of updatedTeam2) {
        if(userCur.PlacementMatchIds === undefined || userCur.PlacementMatchIds === null) {
            userCur.PlacementMatchIds = [];
        }
        if(userCur.PlacementMatchIds.length < 10) {
            userCur.PlacementMatchIds.push(match.Id);
        }
        await update_user_from_ddb(dynamoDb, userCur.Id, userCur);
    }
}

export const calculatePoints = (team1Users: User[], team2Users: User[]): User[][] => {
    // convert user array to rating array

    // const team1 = [new Rating(), new Rating()];
    const team1 = team1Users.map(x => new Rating(x.RoCoMMR,x.RoCoSigma));
    const team2 = team2Users.map(x => new Rating(x.RoCoMMR,x.RoCoSigma));

    // q is quality of the match with the players at their current rating
    const q = quality([team1, team2]);

    // Assumes the first team was the winner by default
    const [rated1, rated2] = rate([team1, team2]); // rate also takes weights of winners or draw
    for (let i = 0; i < rated1.length; i++) {
        const userCur: User = team1Users[i];
        let newMu: number = rated1[i].mu;
        const newSigma: number = rated1[i].sigma;
        const didWin: boolean = userCur.RoCoMMR < newMu;
        const mmrDifference: number = Math.abs(userCur.RoCoMMR - newMu);
        if(userCur.PlacementMatchIds && userCur.PlacementMatchIds.length < 10) {
            if(didWin) {
                newMu += mmrDifference * .20;
            }
            else {
                newMu -= mmrDifference * .20;
            }
        }
        if(!didWin) {
            // if in placements, this yields a loss of 10% less than calculated instead of 20%.
            // if not in placements, yield a loss of 10% less than calculated.
            newMu += mmrDifference * .10;
        }
        userCur.RoCoMMR = newMu;
        userCur.RoCoSigma = Math.max(newSigma, INITIAL_SIGMA * .30);
    }
    for (let i = 0; i < rated2.length; i++) {
        const userCur: User = team2Users[i];
        let newMu: number = rated2[i].mu;
        const newSigma: number = rated2[i].sigma;
        const didWin: boolean = userCur.RoCoMMR < newMu;
        const mmrDifference: number = Math.abs(userCur.RoCoMMR - newMu);
        if(userCur.PlacementMatchIds && userCur.PlacementMatchIds.length < 10) {
            if(didWin) {
                newMu += mmrDifference * .20;
            }
            else {
                newMu -= mmrDifference * .20;
            }
        }
        if(!didWin) {
            // if in placements, this yields a loss of 10% less than calculated instead of 20%.
            // if not in placements, yield a loss of 10% less than calculated.
            newMu += mmrDifference * .10;
        }
        userCur.RoCoMMR = newMu;
        userCur.RoCoSigma = Math.max(newSigma, INITIAL_SIGMA * .30);
    }
    return [team1Users, team2Users];
}

export const getOne = async (event: any, context: Context): Promise<any> => {
    const params = {
        TableName: process.env.DYNAMODB_TABLE,
        Key: {
          Id: event.pathParameters.id,
          EntityType: 'match'
        },
    };

    // fetch user from the database by id
    try {
        const result = await dynamoDb.get(params).promise();
        const response = {
            statusCode: 200,
            body: JSON.stringify(result.Item),
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t fetch the match.',
        };
    }
}

export const getByMatchNum = async (event: any, context: Context): Promise<any> => {
    const params = {
        TableName: process.env.DYNAMODB_TABLE,
        IndexName: 'EntityTypeMatchNumberIndex',
        KeyConditionExpression: 'EntityType = :hashKey AND MatchNumber = :matchNum',
        ExpressionAttributeValues: {
            ':hashKey': 'match',
            ':matchNum': Number.parseInt(event.queryStringParameters.matchNumber, 10)
        },
    };

    // fetch user from the database by id
    try {
        const result = await dynamoDb.query(params).promise();
        if(result.Count && result.Count > 0) {
            // Only grab the first item.
            // Could be improved later to throw an exception if there are
            //  more than 1 match with the same number.
            const retVal = result.Items[0];
            const response = {
                statusCode: 200,
                body: JSON.stringify(retVal),
            };
            return response;
        }
        else {
            throw new Error(JSON.stringify({ error: 'invalid result from dynamodb' , res: result }));
        }
    } catch(error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t fetch the match for matchNumber ' + event.queryStringParameters.matchNumber + '\n Error: ' + error,
        };
    }
};

export const getAll = async (event: any, context: Context): Promise<any> => {
    try {
        const filteredResults = await get_all_matches_from_ddb(dynamoDb);
        const response = {
            statusCode: 200,
            body: JSON.stringify(filteredResults),
        };
        return response;
    } catch (error) {
        throw new Error('Couldn\'t fetch all users: '+error);
    }
};