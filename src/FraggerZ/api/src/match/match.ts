import { DynamoDB } from "aws-sdk";
import { v4 } from "uuid";

export class Match {
    Id: string;
    MatchNumber: number;
    Captain1Id: string;
    Captain2Id: string;
    PickingCaptainId: string;
    MapName: string;
    GameName: string;
    MatchRegion: string;
    MapImageURL: string;
    PlayerIdsPool: string[];
    Team1Ids: string[];
    Team2Ids: string[];
    MatchStatus: MatchStatus;
    MatchType: MatchType;
    WinningTeam: number;
    ReadyUserIds: string[];
    DateTimeStarted: Date;
    DateTimeEnded: Date;
}

export function match_to_ddb(id: string, matchNumber: number, data: any): any {
    return {
        Id: id || v4(),
        EntityType: 'match',
        MatchNumber: matchNumber,
        Captain1Id: data.Captain1Id,
        Captain2Id: data.Captain2Id,
        PickingCaptainId: data.PickingCaptainId,
        MapName: data.MapName,
        GameName: data.GameName,
        MatchRegion: data.MatchRegion,
        MapImageURL: data.MapImageURL,
        PlayerIdsPool: data.PlayerIdsPool,
        Team1Ids: data.Team1Ids,
        Team2Ids: data.Team2Ids,
        MatchStatus: data.MatchStatus,
        MatchType: data.MatchType,
        WinningTeam: data.WinningTeam,
        ReadyUserIds: data.ReadyUserIds,
        DateTimeStarted: data.DateTimeStarted,
        DateTimeEnded: data.DateTimeEnded
    }
}

export function match_to_ddb_update_params(id: string, data: any, tableName: string | undefined): any {
    return {
        TableName: tableName || process.env.DYNAMODB_TABLE,
        Key: {
            Id: id,
            EntityType: 'match'
        },
        UpdateExpression: 'SET MatchNumber = :num'
            + ', Captain1Id = :cap1id'
            + ', Captain2Id = :cap2id'
            + ', PickingCaptainId = :pickcapid'
            + ', MapName = :map'
            + ', GameName = :gname'
            + ', MatchRegion = :reg'
            + ', MapImageURL = :miurl'
            + ', PlayerIdsPool = :pidpool'
            + ', Team1Ids = :t1ids'
            + ', Team2Ids = :t2ids'
            + ', MatchStatus = :status'
            + ', MatchType = :type'
            + ', WinningTeam = :wteam'
            + ', ReadyUserIds = :ruids'
            + ', DateTimeStarted = :dstart'
            + ', DateTimeEnded = :dend',
        ExpressionAttributeValues: {
          ':num': data.MatchNumber || -1,
          ':cap1id': data.Captain1Id || '',
          ':cap2id': data.Captain2Id || '',
          ':pickcapid': data.PickingCaptainId || '',
          ':map': data.MapName || '',
          ':gname': data.GameName || '',
          ':reg': data.MatchRegion || '',
          ':miurl': data.MapImageURL || '',
          ':pidpool': data.PlayerIdsPool || [],
          ':t1ids': data.Team1Ids || [],
          ':t2ids': data.Team2Ids || [],
          ':status': data.MatchStatus || MatchStatus.Unknown,
          ':type': data.MatchType || MatchType.Unknown,
          ':wteam': data.WinningTeam || -1,
          ':ruids': data.ReadyUserIds || [],
          ':dstart': data.DateTimeStarted || '0001-01-01T00:00:00',
          ':dend': data.DateTimeEnded || '0001-01-01T00:00:00'
        },
    }
}

export async function get_all_matches_from_ddb(dynamoDb: DynamoDB.DocumentClient, tableName: string | undefined): Promise<any> {
    try {
        const result = await dynamoDb.query({
            TableName: tableName || process.env.DYNAMODB_TABLE,
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

export enum MatchType {
    Unknown,
    PUG,
    Scrim
}

export enum MatchStatus {
    Unknown,
    Cancelled,
    Reported,
    Reversed,
    Picking,
    Playing
}