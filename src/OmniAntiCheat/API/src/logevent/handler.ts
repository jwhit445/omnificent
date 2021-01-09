import { Context } from 'aws-lambda';
import { DynamoDB } from 'aws-sdk'

const dynamoDb = new DynamoDB.DocumentClient()

export const getS3Url = async (event: any, context: Context): Promise<any> => {
    try {
        // todo stuff
        const response = {
            statusCode: 200,
            body: 'SUCCESS getS3Url!',
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t get s3 url.',
        }
    }
};

export const getMany = async (event: any, context: Context): Promise<any> => {
    try {
        // todo stuff
        const response = {
            statusCode: 200,
            body: 'SUCCESS getMany!',
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t get log events for the given list.',
        }
    }
};