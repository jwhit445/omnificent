import { Context } from 'aws-lambda';
import { DynamoDB } from 'aws-sdk';
import { DocumentClient } from 'aws-sdk/clients/dynamodb';
import { v4 } from "uuid";
import { User } from './user';

const dynamoDb = new DynamoDB.DocumentClient()

export const upsert = async (event: any, context: Context): Promise<any> => {
    try {
        const data = JSON.parse(event.body)
        const user = {
            Username: data.User.Username,
            ID: data.User.ID,
            PlatformCode: data.User.PlatformCode
        };
        try {
            const sessionToken: string = v4();
            await dynamoDb.update({
                TableName: process.env.DYNAMODB_TABLE,
                Key: {
                    PK: `#USER#${user.PlatformCode}#${user.ID}`,
                    SK: `PROFILE#${user.Username}`
                },
                UpdateExpression: 'set '
                    + 'ID = :id, '
                    + 'Username = :username, '
                    + 'AuthToken = :authToken, '
                    + 'PlatformCode = :pcode',
                ExpressionAttributeValues: {
                    ':id': user.ID,
                    ':username': user.Username,
                    ':authToken': sessionToken,
                    ':pcode': user.PlatformCode
                }
            }).promise();
            // create a response
            const response = {
                statusCode: 200,
                body: JSON.stringify({ AuthorizationToken: sessionToken }),
            }
            return response;
        } catch (error) {
            throw new Error(new Error('Couldn\'t create the user for Username:' + user.Username) + '\r\nError: ' + error.message);
        }
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t upsert the user.\r\n' + error.message,
        }
    }
};

export const report = async (event: any, context: Context): Promise<any> => {
    try {
        const data = JSON.parse(event.body)
        const reporter: string = data.Reporter;
        const username: string = data.Username;
        const userProfileResult = await getUserProfileFromUserName(username);
        if(!userProfileResult || userProfileResult.Count === 0) {
            throw new Error('Couldn\'t retrieve user for username:' + username);
        }
        const userProfile = userProfileResult.Items[0];
        const dtNow: Date = new Date();
        const reportevent = {
            PK: `#USER#${userProfile.PlatformCode}#${userProfile.Id}`,
            SK: `REPORTEVENT#${dtNow.toISOString()}`,
            Reporter: reporter,
            Status: ReportStatus.Pending,
            DateTimeStatusChanged: dtNow.toISOString(),
        };
        await dynamoDb.put({
            TableName: process.env.DYNAMODB_TABLE,
            Item: reportevent
        }).promise();
        const response = {
            statusCode: 200,
            body: `User ${username} successfully reported by ${reporter}.`,
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t report the given user. \n' + error.message,
        }
    }
};

export interface HeaderInfo {
    IsValid: boolean;
    Id: string;
    Username: string;
    PlatformCode: string;
    Token: string;
    Error: any;
}

interface UserStatus {
    LastHeartbeat: Date;
    IsMossRunning: boolean;
    IsRogueCompanyRunning: boolean;
    ReportStatus: ReportStatus;
}

export const validateAuthHeader = async (authHeader: string): Promise<HeaderInfo> => {
    if(!authHeader || authHeader.trim().length === 0 || !authHeader.startsWith('Bearer ')) {
        return {
            IsValid: false,
            Id: '',
            PlatformCode: '',
            Username: '',
            Token: authHeader,
            Error: null
        };
    }
    try {
        const authSections: string[] = Buffer.from(authHeader.split(" ")[1], 'base64').toString().split(":");
        const idAndPlatform = authSections[0].split("-");
        const platformCode: string = idAndPlatform[0];
        const id: string = idAndPlatform[1];
        const token: string = authSections[1];
        const res = await dynamoDb.query({
            TableName: process.env.DYNAMODB_TABLE,
            KeyConditionExpression: 'PK = :pk AND begins_with(SK, :sk) ',
            ExpressionAttributeValues: {
              ':pk': `#USER#${platformCode}#${id}`,
              ':sk': 'PROFILE#'
            },
            Limit: 1
        }).promise();
        if(res && res.Count > 0) {
            for(const item of res.Items) {
                if(item.AuthToken === token) {
                    return {
                        IsValid: true,
                        Id: id,
                        PlatformCode: platformCode,
                        Username: res.Items[0].Username,
                        Token: token,
                        Error: null
                    };
                }
            }
        }
        return {
            IsValid: false,
            Id: id,
            PlatformCode: platformCode,
            Username: '',
            Token: token,
            Error: JSON.stringify({Count: res.Count})
        };
    }
    catch(err) {
        return {
            IsValid: false,
            Id: '',
            PlatformCode: '',
            Username: '',
            Token: authHeader + ' : ' + authHeader.substr(7),
            Error: err.message
        };
    }

}

export const updateInfo = async (event: any, context: Context): Promise<any> => {
    const headerInfo: HeaderInfo = await validateAuthHeader(event.headers.Authorization);
    if(!headerInfo.IsValid) {
        return {
            statusCode: 401,
            body: '',
        }
    }
    try {
        const data = JSON.parse(event.body);
        const userInfo = {
            IsMossRunning: data.IsMossRunning,
            IsGameRunning: data.IsGameRunning,
            GameType: data.GameType,
        };
        await dynamoDb.update({
            TableName: process.env.DYNAMODB_TABLE,
            Key: {
                PK: `#USER#${headerInfo.PlatformCode}#${headerInfo.Id}`,
                SK: `PROFILE#${headerInfo.Username}`
            },
            UpdateExpression: 'set IsMossRunning = :mossRunning, '
                + 'IsGameRunning = :gameRunning, '
                + 'GameType = :gameType, '
                + 'LastHeartbeat = :heartbeat',
            ExpressionAttributeValues: {
                ':mossRunning': userInfo.IsMossRunning,
                ':gameRunning': userInfo.IsGameRunning,
                ':gameType': userInfo.GameType,
                ':heartbeat': new Date().toISOString()
            }
        }).promise();
        const response = {
            statusCode: 200,
            body: 'Updated the user\'s info',
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t update the user\'s info.\n' + error,
        }
    }
};

const getUserProfileFromUserName = async (userName: string): Promise<DocumentClient.QueryOutput> => {
    return await dynamoDb.query({
        TableName: process.env.DYNAMODB_TABLE,
        IndexName: 'InverseKeyIndex',
        KeyConditionExpression: 'SK = :sk AND begins_with(PK, :pk) ',
        ExpressionAttributeValues: {
          ':sk': `PROFILE#${userName}`,
          ':pk': `#USER#`
        },
        Limit: 1
    }).promise();
};

export const getStatus = async (event: any, context: Context): Promise<any> => {
    try {
        const data = JSON.parse(event.body);
        const user: User = data.User;
        if(user === null || user === undefined) {
            return {
                statusCode: 400,
                headers: { 'Content-Type': 'text/plain' },
                body: 'Invalid user provided.',
            };
        }
        const userProfileResult = await dynamoDb.query({
            TableName: process.env.DYNAMODB_TABLE,
            KeyConditionExpression: 'PK = :pk AND SK = :sk ',
            ExpressionAttributeValues: {
                ':pk': `#USER#${user.PlatformCode}#${user.Id}`,
                ':sk': `PROFILE#${user.Username}`
            },
            Limit: 1
        }).promise();
        if(!userProfileResult || userProfileResult.Count === 0) {
            throw new Error(`User profile not found for user: ${user.Username}`);
        }
        const userProfile = userProfileResult.Items[0];
        const retVal: UserStatus = {
            IsMossRunning: userProfile.IsMossRunning,
            IsRogueCompanyRunning: userProfile.IsRogueCompanyRunning,
            LastHeartbeat: userProfile.LastHeartbeat,
            ReportStatus: ReportStatus.NotReported,
        };
        const reportResults = await dynamoDb.query({
            TableName: process.env.DYNAMODB_TABLE,
            KeyConditionExpression: 'PK = :pk AND begins_with(SK, :sk) ',
            ExpressionAttributeValues: {
                ':pk': `#USER#${user.PlatformCode}#${user.Id}`,
                ':sk': `REPORTEVENT#`
            },
            ScanIndexForward: false
        }).promise();
        if(reportResults !== null && reportResults !== undefined && reportResults.Items.length > 0) {
            retVal.ReportStatus = reportResults.Items[0].Status;
        }
        const response = {
            statusCode: 200,
            body: JSON.stringify({ Status: retVal }),
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t get the statuses for the given user.\n' + error,
        }
    }
};

export const getStatuses = async (event: any, context: Context): Promise<any> => {
    try {
        const data = JSON.parse(event.body);
        const req = {
            ListUsernames: data.ListUsernames,
        };
        // For now, force usernames as they are easier to retrieve from Epic.
        if(req.ListUsernames === null || req.ListUsernames === undefined) {
            return {
                statusCode: 400,
                headers: { 'Content-Type': 'text/plain' },
                body: 'No usernames provided.',
            };
        }
        const retVal: any = {};
        for(const userName of req.ListUsernames) {
            const result = await getUserProfileFromUserName(userName);
            if(!result || result.Count === 0) {
                continue;
            }
            retVal[userName] = [];
            for(const item of result.Items) {
                const reportResults = await dynamoDb.query({
                    TableName: process.env.DYNAMODB_TABLE,
                    KeyConditionExpression: 'PK = :pk AND begins_with(SK, :sk) ',
                    ExpressionAttributeValues: {
                        ':pk': `#USER#${item.PlatformCode}#${item.ID}`,
                        ':sk': `REPORTEVENT#`
                    },
                    ScanIndexForward: false
                }).promise();
                let reportStatus: ReportStatus = ReportStatus.NotReported;
                if(reportResults !== null && reportResults !== undefined && reportResults.Items.length > 0) {
                    reportStatus = reportResults.Items[0].Status;
                }
                retVal[userName] = {
                    IsMossRunning: item.IsMossRunning,
                    IsGameRunning: item.IsGameRunning,
                    LastHeartbeat: item.LastHeartbeat,
                    GameType: item.GameType,
                    ReportStatus: reportStatus,
                };
            }
        }
        const response = {
            statusCode: 200,
            body: JSON.stringify({ UserStatuses: retVal }),
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t get the statuses for the given user.\n' + error,
        }
    }
};

export enum ReportStatus {
    NotReported,
    Pending,
    Cleared,
    Confirmed,
}