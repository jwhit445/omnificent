import { Context } from 'aws-lambda';
import { DynamoDB } from 'aws-sdk';
import { v4 } from "uuid";
import { User } from './user';

const dynamoDb = new DynamoDB.DocumentClient()

export const upsert = async (event: any, context: Context): Promise<any> => {
    try {
        const data = JSON.parse(event.body)
        const user = {
            Username: data.User.Username,
            EpicID: data.User.EpicID,
        };
        try {
            const sessionToken: string = v4();
            await dynamoDb.update({
                TableName: process.env.DYNAMODB_TABLE,
                Key: {
                    PK: `#USER#${user.EpicID}`,
                    SK: `PROFILE`
                },
                UpdateExpression: 'set '
                    + 'EpicID = :id, '
                    + 'Username = :username, '
                    + 'AuthToken = :authToken',
                ExpressionAttributeValues: {
                    ':id': user.EpicID,
                    ':username': user.Username,
                    ':authToken': sessionToken
                }
            }).promise();
            // create a response
            const response = {
                statusCode: 200,
                body: JSON.stringify({ AuthorizationToken: sessionToken }),
            }
            return response;
        } catch (error) {
            throw new Error('Couldn\'t create the match for Username:' + user.Username);
        }
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t upsert the user.',
        }
    }
};

interface HeaderInfo {
    IsValid: boolean;
    Id: string;
    Token: string;
    Error: any;
}

const validateAuthHeader = async (authHeader: string): Promise<HeaderInfo> => {
    if(!authHeader || authHeader.trim().length === 0 || !authHeader.startsWith('Bearer ')) {
        return { IsValid: false, Id: '', Token: authHeader, Error: null };
    }
    try {
        const authSections: string[] = Buffer.from(authHeader.split(" ")[1], 'base64').toString().split(":");
        const id: string = authSections[0];
        const token: string = authSections[1];
        const res = await dynamoDb.query({
            TableName: process.env.DYNAMODB_TABLE,
            KeyConditionExpression: 'PK = :pk AND SK = :sk ',
            ExpressionAttributeValues: {
              ':pk': `#USER#${id}`,
              ':sk': 'PROFILE'
            },
            Limit: 1
        }).promise();
        if(!res || res.Count === 0 || res.Items[0].AuthToken !== token) {
            return { IsValid: false, Id: id, Token: token, Error: JSON.stringify({Count: res.Count}) };
        }
        return { IsValid: true, Id: id, Token: token, Error: null };
    }
    catch(err) {
        return { IsValid: false, Id: '', Token: authHeader + ' : ' + authHeader.substr(7), Error: err.message };
    }

}

export const updateInfo = async (event: any, context: Context): Promise<any> => {
    const headerInfo: HeaderInfo = await validateAuthHeader(event.headers.Authorization);
    if(!headerInfo.IsValid) {
        return {
            statusCode: 401,
            body: JSON.stringify(headerInfo),
        }
    }
    try {
        const data = JSON.parse(event.body);
        const userInfo = {
            IsMossRunning: data.IsMossRunning,
            IsRogueCompanyRunning: data.IsRogueCompanyRunning
        };
        await dynamoDb.update({
            TableName: process.env.DYNAMODB_TABLE,
            Key: {
                PK: `#USER#${headerInfo.Id}`,
                SK: `PROFILE`
            },
            UpdateExpression: 'set IsMossRunning = :mossRunning, '
                + 'IsRogueCompanyRunning = :rocoRunning, '
                + 'LastHeartbeat = :heartbeat',
            ExpressionAttributeValues: {
                ':mossRunning': userInfo.IsMossRunning,
                ':rocoRunning': userInfo.IsRogueCompanyRunning,
                ':heartbeat': new Date().toUTCString()
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

export const getStatuses = async (event: any, context: Context): Promise<any> => {
    try {
        const data = JSON.parse(event.body);
        const listUsers: User[] = data.ListUsers;
        const tableName = process.env.DYNAMODB_TABLE;
        const listKeys = listUsers.map(x => ({ PK: `#USER#${x.EpicID}`, SK: `PROFILE`}));
        const params = {
            RequestItems: {
                [tableName]: {
                    Keys: listKeys,
                }
            },
        };
        const res = await dynamoDb.batchGet(params).promise();
        const retVal: any = {};
        for(const item of res.Responses[tableName]) {
            retVal[item.EpicID] = {
                IsMossRunning: item.IsMossRunning,
                IsRogueCompanyRunning: item.IsRogueCompanyRunning,
                LastHeartbeat: item.LastHeartbeat
            };
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