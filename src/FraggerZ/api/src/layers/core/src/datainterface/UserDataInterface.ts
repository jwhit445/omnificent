// This file is auto-generated.

import DynamoDB from 'aws-sdk/clients/dynamodb';
import { User } from "../models/User";
import { UserPK } from "./models/UserPK";

export async function getDB(dynamoDb: DynamoDB.DocumentClient, pk: UserPK): Promise<User[]> {
	if(pk.ServerId === null || pk.ServerId === undefined) {
		throw new Error("PK property: ServerId is not set.");
	}
	if(pk.UserId === null || pk.UserId === undefined) {
		throw new Error("PK property: UserId is not set.");
	}
	let isPartialSK = true;
	const pkKey = `#USER#${pk.ServerId}#${pk.UserId}`;
	let skKey = '#USER#';
	if(pk.ServerId !== null && pk.ServerId !== undefined) {
		skKey += `${pk.ServerId}`;
		if(pk.UserId !== null && pk.UserId !== undefined) {
			skKey += `#${pk.UserId}`;
			isPartialSK = false;
		}
	}
	const params: any = {
		TableName: process.env.DYNAMODB_TABLE,
		KeyConditionExpression: `PK = :pkKey AND ${isPartialSK ? "begins_with(SK, :skKey)" : "SK = :skKey"}`,
		ExpressionAttributeValues: {
			':pkKey': pkKey,
			':skKey': skKey,
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
	} while(params.ExclusiveStartKey !== undefined && params.ExclusiveStartKey !== null);
	return retVal;
}

