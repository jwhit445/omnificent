import { v4 } from "uuid";

export class Team {
    Id: string;
    CaptainId: string;
    TeamName: string;
    GameName: string;
    MemberIds: string[];
    MatchStatIds: string[];
}

export function team_to_ddb(data: any): any {
    return {
        Id: data.Id,
        EntityType: 'team',
        CaptainId: data.CaptainId,
        TeamName: data.TeamName,
        GameName: data.GameName,
        MemberIds: data.MemberIds,
        MatchStatIds: data.MatchStatIds,
    }
}

export function team_to_ddb_update_params(id: string, data: any): any {
    return {
        TableName: process.env.DYNAMODB_TABLE,
        Key: {
            Id: id,
            EntityType: 'team'
        },
        UpdateExpression: 'SET CaptainId = :capid'
            + ', TeamName = :name'
            + ', GameName = :gname'
            + ', MemberIds = :memids'
            + ', MatchStatIds = :mrecids',
        ExpressionAttributeValues: {
          ':capid': data.CaptainId || -1,
          ':name': data.TeamName || '',
          ':gname': data.GameName || '',
          ':memids': data.MemberIds || '',
          ':mrecids': data.MatchStatIds || ''
        },
    }
}