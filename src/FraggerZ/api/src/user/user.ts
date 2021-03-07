import { DynamoDB } from 'aws-sdk'
import { ExpressionAttributeValueMap, ItemList, QueryInput } from 'aws-sdk/clients/dynamodb';

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

export async function get_user_from_ddb(dynamoDb: DynamoDB.DocumentClient, id: string): Promise<any> {
    const params = {
        TableName: process.env.DYNAMODB_TABLE,
        Key: {
          Id: id,
          EntityType: 'user'
        },
    };
    // fetch user from the database by id
    const result = await dynamoDb.get(params).promise();
    return result.Item;
}

export async function update_user_from_ddb(dynamoDb: DynamoDB.DocumentClient, id: string, user: User): Promise<any> {
    const params = user_to_ddb_update_params(id, user);
    try {
        // write the match changes to the database
        return await dynamoDb.update(params).promise();
    } catch (error) {
        throw new Error('Couldn\'t update the user:' + JSON.stringify(user) + '\r\nError: ' + error);
    }
};

export async function get_all_from_ddb(dynamoDb: DynamoDB.DocumentClient): Promise<any> {
    try {
        const params: any = {
            TableName: process.env.DYNAMODB_TABLE,
            IndexName: 'InverseKey',
            KeyConditionExpression: 'EntityType = :hashKey',
            ExpressionAttributeValues: {
                ':hashKey': 'user',
            },
            ExclusiveStartKey: undefined,
        };
        const retVal: any[] = [];
        do {
            const result = await dynamoDb.query(params).promise();
            if(result.Items) {
                retVal.push(...result.Items);
            }
            params.ExclusiveStartKey = result.LastEvaluatedKey;
        }
        while((params.ExclusiveStartKey !== undefined && params.ExclusiveStartKey !== null));
        return retVal;
    } catch (error) {
        throw new Error('Couldn\'t fetch all users: '+error);
    }
};

export async function get_all_non_reset_from_ddb(dynamoDb: DynamoDB.DocumentClient, newMMR: number, newSigma: number): Promise<any> {
    try {
        const params: any = {
            TableName: process.env.DYNAMODB_TABLE,
            IndexName: 'InverseKey',
            KeyConditionExpression: 'EntityType = :hashKey',
            FilterExpression: "RoCoMMR <> :mmr AND RoCoSigma <> :sigma AND size(PlacementMatchIds) > :zero",
            ExpressionAttributeValues: {
                ':hashKey': 'user',
                ':mmr': newMMR.toString(),
                ':sigma': newSigma.toString(),
                ':zero': 0,
            },
            ExclusiveStartKey: undefined,
        };
        const retVal: any[] = [];
        do {
            const result = await dynamoDb.query(params).promise();
            if(result.Items) {
                retVal.push(...result.Items);
            }
            params.ExclusiveStartKey = result.LastEvaluatedKey;
        }
        while((params.ExclusiveStartKey !== undefined && params.ExclusiveStartKey !== null));
        return retVal;
    } catch (error) {
        throw new Error('Couldn\'t fetch all non-reset users: '+error);
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

export function user_to_ddb_update_params(id: string, data: any): any {
    return {
        TableName: process.env.DYNAMODB_TABLE,
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

export async function get_user_count_above_rocommr(dynamoDb: DynamoDB.DocumentClient, mmr: number): Promise<any> {
    try {
        const result = await dynamoDb.query({
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