// This file is auto-generated.

import DynamoDB from 'aws-sdk/clients/dynamodb';
import { UserGameProfile } from "../models/UserGameProfile";
import { UserPK } from "./models/UserPK";
import { UserGameProfileSK } from "./models/UserGameProfileSK";

export async function getDB(dynamoDb: DynamoDB.DocumentClient, pk: UserPK, sk: UserGameProfileSK): Promise<UserGameProfile[]> {
	if(pk.ServerId === null || pk.ServerId === undefined) {
		throw new Error("PK property: ServerId is not set.");
	}
	if(pk.UserId === null || pk.UserId === undefined) {
		throw new Error("PK property: UserId is not set.");
	}
	let isPartialSK = true;
	const pkKey = `#USER#${pk.ServerId}#${pk.UserId}`;
	let skKey = 'GAMEPROFILE#';
	if(sk.GameCode !== null && sk.GameCode !== undefined) {
		skKey += `${sk.GameCode}`;
		isPartialSK = false;
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

export async function getDBInverse(dynamoDb: DynamoDB.DocumentClient, pk: UserGameProfileSK, sk: UserPK): Promise<UserGameProfile[]> {
	if(pk.GameCode === null || pk.GameCode === undefined) {
		throw new Error("PK property: GameCode is not set.");
	}
	let isPartialSK = true;
	const pkKey = `GAMEPROFILE#${pk.GameCode}`;
	let skKey = '#USER#';
	if(sk.ServerId !== null && sk.ServerId !== undefined) {
		skKey += `${sk.ServerId}`;
		if(sk.UserId !== null && sk.UserId !== undefined) {
			skKey += `#${sk.UserId}`;
			isPartialSK = false;
		}
	}
	const params: any = {
		TableName: process.env.DYNAMODB_TABLE,
		IndexName: 'InverseIndex',
		KeyConditionExpression: `SK = :pkKey AND ${isPartialSK ? "begins_with(PK, :skKey)" : "PK = :skKey"}`,
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
