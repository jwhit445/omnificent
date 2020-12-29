import { DynamoDB } from 'aws-sdk'

export class User {
    Id: string;
    Username: string;
    RoCoMMR: number;
    RoCoSigma: number;
    CrossFireMMR: number;
    IronSightMMR: number;
    SuspensionReturnDate: Date;
    MatchStatIds: string[];
    TeamIds: string[];
    StreamURL: string;
    IGN: string;
    PlacementMatchIds: string[];
    WinStreakMatchIds: string[];
    DateTimeLastMapVote: Date;
    DateTimeLastScrambleVote: Date;
    DateTimeLastStatReset: Date;
}

export const get_user_from_ddb = async (dynamoDb: DynamoDB.DocumentClient, id: string, tableName: string | undefined): Promise<any> => {
    const params = {
        TableName: tableName || process.env.DYNAMODB_TABLE,
        Key: {
          Id: id,
          EntityType: 'user'
        },
    };
    // fetch user from the database by id
    const result = await dynamoDb.get(params).promise();
    return result.Item;
}

export const update_user_from_ddb = async (dynamoDb: DynamoDB.DocumentClient, id: string, user: User, tableName: string | undefined): Promise<any> => {
    const params = user_to_ddb_update_params(id, user, tableName);
    try {
        // write the match changes to the database
        return await dynamoDb.update(params).promise();
    } catch (error) {
        throw new Error('Couldn\'t update the user:' + JSON.stringify(user) + '\r\nError: ' + error);
    }
};

export const get_all_from_ddb = async (dynamoDb: DynamoDB.DocumentClient, tableName: string | undefined): Promise<any> => {
    const params = {
        TableName: tableName || process.env.DYNAMODB_TABLE,
        FilterExpression: 'EntityType = :et',
        ExpressionAttributeValues: {
            ':et': 'user'
        }
    };
    try {
        var result = await dynamoDb.scan(params).promise();
        if(result.Count && result.Items) {
            return result.Items;
        }
        else {
            throw new Error('No user results found in db');
        }
    } catch (error) {
        throw new Error('Couldn\'t fetch all users: '+error);
    }
};

export function user_to_ddb(data: any): any {
    return {
        Id: data.Id,
        EntityType: 'user',
        Username: data.Username,
        RoCoMMR: data.RoCoMMR,
        RoCoSigma: data.RoCoSigma,
        CrossFireMMR: data.CrossFireMMR,
        IronSightMMR: data.IronSightMMR,
        SuspensionReturnDate: data.SuspensionReturnDate,
        MatchStatIds: data.MatchStatIds,
        TeamIds: data.TeamIds,
        StreamURL: data.StreamURL,
        IGN: data.IGN,
        PlacementMatchIds: data.PlacementMatchIds,
        WinStreakMatchIds: data.WinStreakMatchIds,
        DateTimeLastMapVote: data.DateTimeLastMapVote,
        DateTimeLastScrambleVote: data.DateTimeLastScrambleVote,
        DateTimeLastStatReset: data.DateTimeLastStatReset,
    }
}

export function user_to_ddb_update_params(id: string, data: any, tableName: string | undefined): any {
    return {
        TableName: tableName || process.env.DYNAMODB_TABLE,
        Key: {
            Id: id,
            EntityType: 'user'
        },
        UpdateExpression: 'SET Username = :uname, RoCoMMR = :rocommr, CrossFireMMR = :cfmmr, '
            + 'IronSightMMR = :ismmr, SuspensionReturnDate = :srdate, MatchStatIds = :mrids, TeamIds = :tids, '
            + 'StreamURL = :surl, IGN = :ign'
            + ', PlacementMatchIds = :pmatchid'
            + ', WinStreakMatchIds = :wstreakid'
            + ', RoCoSigma = :rocosigma'
            + ', DateTimeLastMapVote = :dmap'
            + ', DateTimeLastScrambleVote = :dscram'
            + ', DateTimeLastStatReset = :dstat',
        ExpressionAttributeValues: {
          ':uname': data.Username,
          ':rocommr': data.RoCoMMR,
          ':cfmmr': Math.round(data.CrossFireMMR * 100) / 100,
          ':ismmr': Math.round(data.IronSightMMR * 100) / 100,
          ':srdate': data.SuspensionReturnDate,
          ':mrids': data.MatchStatIds || [],
          ':tids': data.TeamIds,
          ':surl': data.StreamURL,
          ':ign': data.IGN,
          ':pmatchid': data.PlacementMatchIds || [],
          ':wstreakid': data.WinStreakMatchIds || [],
          ':rocosigma': Math.round(data.RoCoSigma * 100) / 100 || -1,
          ':dmap': data.DateTimeLastMapVote || '0001-01-01T00:00:00',
          ':dscram': data.DateTimeLastScrambleVote || '0001-01-01T00:00:00',
          ':dstat': data.DateTimeLastStatReset || '0001-01-01T00:00:00'
        },
    }
}

export const get_user_count_above_rocommr = async (dynamoDb: DynamoDB.DocumentClient, mmr: number): Promise<any> => {
    try {
        var result = await dynamoDb.query({
            TableName: process.env.DYNAMODB_TABLE,
            IndexName: 'EntityTypeMatchNumberIndex',
            KeyConditionExpression: 'EntityType = :hashKey',
            ExpressionAttributeValues: {
              ':hashKey': 'match',
            },
            ScanIndexForward: true // true or false to sort by MatchNumber Sort/Range key ascending or descending
        })
        .promise();
        if(result.Count && result.Items) {
            return result.Items;
        }
        else {
            throw new Error('No match results found in db');
        }
    } catch (error) {
        throw new Error('Couldn\'t fetch all matches: '+error);
    }
};