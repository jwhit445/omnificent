import { Context } from 'aws-lambda';
import { DynamoDB } from 'aws-sdk'
import { HeaderInfo, validateAuthHeader } from '../user/handler';

const dynamoDb = new DynamoDB.DocumentClient();

export const create = async (event: any, context: Context): Promise<any> => {
    const headerInfo: HeaderInfo = await validateAuthHeader(event.headers.Authorization);
    if(!headerInfo.IsValid) {
        return {
            statusCode: 401,
            body: '',
        }
    }
    try {
        const data = JSON.parse(event.body);
        const dtEnd = new Date().toISOString();
        const logevent = {
            PK: `#USER#${headerInfo.PlatformCode}#${headerInfo.Id}`,
            SK: `LOGEVENT#${dtEnd}`,
            DateTimeStarted: data.DateTimeStarted,
            DateTimeEnded: dtEnd,
            S3Url: data.S3Url,
        };
        await dynamoDb.put({
            TableName: process.env.DYNAMODB_TABLE,
            Item: logevent
        }).promise();
        const response = {
            statusCode: 200,
            body: 'Logevent successfully created.',
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t create logevent.',
        }
    }
};

export const getMany = async (event: any, context: Context): Promise<any> => {
    try {
        const data = JSON.parse(event.body);
        const req = {
            ListUsername: data.ListUsername,
            ListEpicID: data.ListEpicID,
        };
        // For now, force usernames as they are easier to retrieve from Epic.
        if(req.ListUsername === null || req.ListUsername === undefined) {
            return {
                statusCode: 400,
                headers: { 'Content-Type': 'text/plain' },
                body: 'No usernames provided.',
            };
        }
        const retVal: any = {};
        for(const userName of req.ListUsername) {
            let result = await dynamoDb.query({
                TableName: process.env.DYNAMODB_TABLE,
                IndexName: 'InverseKeyIndex',
                KeyConditionExpression: 'SK = :sk AND begins_with(PK, :pk) ',
                ExpressionAttributeValues: {
                  ':sk': `PROFILE#${userName}`,
                  ':pk': '#USER#'
                },
                Limit: 1
            }).promise();
            if(!result || result.Count === 0) {
                continue;
            }
            retVal[userName] = [];
            for(const userItem of result.Items) {
                result = await dynamoDb.query({
                    TableName: process.env.DYNAMODB_TABLE,
                    KeyConditionExpression: 'PK = :pk AND begins_with(SK, :sk) ',
                    ExpressionAttributeValues: {
                      ':pk': `#USER#${userItem.PlatformCode}#${userItem.ID}`,
                      ':sk': 'LOGEVENT#'
                    },
                    ScanIndexForward: false, // Order newest to oldest
                    Limit: 5,
                }).promise();
                if(!result || result.Count === 0) {
                    continue;
                }
                for(const logevent of result.Items) {
                    retVal[userName].push({
                        PlatformCode: userItem.PlatformCode,
                        StartDateTime: logevent.DateTimeStarted,
                        EndDateTime: logevent.DateTimeEnded,
                        DownloadLink: logevent.S3Url,
                    });
                }
            }
        }

        const response = {
            statusCode: 200,
            body: JSON.stringify({ RecentUserEvents: retVal }),
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t get log events for the given list.\nError: ' + error,
        };
    }
};