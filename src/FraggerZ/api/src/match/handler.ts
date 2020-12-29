import { Handler, Context } from 'aws-lambda';
import { DynamoDB } from 'aws-sdk'
import { rate, Rating, quality } from 'ts-trueskill';
import { get_all_matches_from_ddb, Match, MatchStatus, match_to_ddb, match_to_ddb_update_params } from './match';
import { v4 } from 'uuid';
import { get_user_from_ddb, update_user_from_ddb, User, user_to_ddb_update_params } from '../user/user';

const dynamoDb = new DynamoDB.DocumentClient();

module.exports.register = async (event: any, context: Context): Promise<any> => {
    const data = JSON.parse(event.body)
    try {
        if (!validateMatch(data)) {
            console.error('Validation Failed')
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
            //No record in the DB. Start with 1.
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
            console.error('Validation Failed')
            callback(new Error("Invalid request data"));
            return;
        }
    } catch(error) {
        callback(new Error("Invalid request data"));
        return;
    }

    const params = match_to_ddb_update_params(event.pathParameters.id, data, undefined);

    // write the match changes to the database
    dynamoDb.update(params, (error, result) => {
        // handle potential errors
        if (error) {
            console.error(error);
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
        console.error('Validation Failed')
        throw new Error("Invalid request data");
    }
    var match = data as Match;
    if(match.MatchStatus == MatchStatus.Reported) {
        throw new Error("This match has already been reported");
    }
    if(match.MatchStatus == MatchStatus.Cancelled) {
        throw new Error("This match was cancelled!");
    }
    try {
        await reportMatch(match, event.pathParameters.id, undefined);
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

export const reportMatch = async (match: Match, id: string, tableName: string): Promise<any> => {
    match.MatchStatus = MatchStatus.Reported;
    // Get all users and their current mmr
    var team1Users: User[] = [];
    var team2Users: User[] = [];
    // For each user, call an "assign points" method that takes in:
    // 1. The user id and 
    // 2. If they won or lost
    // 3. All of the opposing team's
    for (let i = 0; i < match.Team1Ids.length; i++) {
        const userId = match.Team1Ids[i];
        // Get the user from their id
        const userCur: User = await get_user_from_ddb(dynamoDb, userId, tableName);
        team1Users.push(userCur);
    }
    for (let i = 0; i < match.Team2Ids.length; i++) {
        const userId = match.Team2Ids[i];
        // Get the user from their id
        const userCur: User = await get_user_from_ddb(dynamoDb, userId, tableName);
        team2Users.push(userCur);
    }
    if(match.WinningTeam == 1) {
        const [updatedTeam1, updatedTeam2] = calculatePoints(team1Users, team2Users);
        await updateTeamUsers(match, updatedTeam1, updatedTeam2);
    }
    else if(match.WinningTeam == 2) {
        const [updatedTeam2, updatedTeam1] = calculatePoints(team2Users, team1Users);
        await updateTeamUsers(match, updatedTeam1, updatedTeam2);
    }
    else {
        throw new Error('Invalid winning team.');
    }
    const params = match_to_ddb_update_params(id, match, tableName);
    // write the match changes to the database
    await dynamoDb.update(params).promise();
}

const updateTeamUsers = async (match: Match, updatedTeam1: User[], updatedTeam2: User[]): Promise<any> => {
    for (let i = 0; i < updatedTeam1.length; i++) {
        const userCur = updatedTeam1[i];
        if(userCur.PlacementMatchIds === undefined || userCur.PlacementMatchIds === null) {
            userCur.PlacementMatchIds = [];
        }
        if(userCur.PlacementMatchIds.length < 10) {
            userCur.PlacementMatchIds.push(match.Id);
        }
        await update_user_from_ddb(dynamoDb, userCur.Id, userCur, undefined);
    }
    for (let i = 0; i < updatedTeam2.length; i++) {
        const userCur = updatedTeam2[i];
        if(userCur.PlacementMatchIds === undefined || userCur.PlacementMatchIds === null) {
            userCur.PlacementMatchIds = [];
        }
        if(userCur.PlacementMatchIds.length < 10) {
            userCur.PlacementMatchIds.push(match.Id);
        }
        await update_user_from_ddb(dynamoDb, userCur.Id, userCur, undefined);
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
        team1Users[i].RoCoMMR = rated1[i].mu;
        team1Users[i].RoCoSigma = rated1[i].sigma;
    }
    for (let i = 0; i < rated2.length; i++) {
        team2Users[i].RoCoMMR = rated2[i].mu;
        team2Users[i].RoCoSigma = rated2[i].sigma;
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
        FilterExpression: 'MatchNumber = :matchNum',
        ExpressionAttributeValues: {
            ':matchNum': parseInt(event.queryStringParameters.matchNumber)
        }
    };
    
    // fetch user from the database by id
    try {
        const result = await dynamoDb.scan(params).promise();
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
    } catch(error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t fetch the match for matchNumber ' + event.queryStringParameters.matchNumber,
        };
    }
};

export const getAll = async (event: any, context: Context): Promise<any> => {
    try {
        var filteredResults = await get_all_matches_from_ddb(dynamoDb, undefined);
        const response = {
            statusCode: 200,
            body: JSON.stringify(filteredResults),
        };
        return response;
    } catch (error) {
        throw new Error('Couldn\'t fetch all users: '+error);
    }
};