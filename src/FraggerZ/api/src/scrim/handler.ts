import { Handler, Context } from 'aws-lambda';
import { DynamoDB } from 'aws-sdk'
import { UserMatch } from '../models/UserMatch';

const dynamoDb = new DynamoDB.DocumentClient()

export const create: Handler = (event: any, context: Context, callback: any) => {
    callback(null, {
        statusCode: 200,
        body: "Successful create!"
    });
};

export const getByMessageId: Handler = (event: any, context: Context, callback: any) => {
    callback(null, {
        statusCode: 200,
        body: "Successful getByMessageId!"
    });
};

export const getByTeams: Handler = (event: any, context: Context, callback: any) => {
    callback(null, {
        statusCode: 200,
        body: "Successful getByTeams!"
    });
};

export const deleteScrim: Handler = (event: any, context: Context, callback: any) => {
    callback(null, {
        statusCode: 200,
        body: "Successful deleteScrim!"
    });
};