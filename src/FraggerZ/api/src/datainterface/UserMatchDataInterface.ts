// This file is auto-generated.

import DynamoDB from 'aws-sdk/clients/dynamodb';
import { UserMatch } from "../models/UserMatch";
import { UserPK } from "./models/UserPK";
import { UserMatchSK } from "./models/UserMatchSK";

export async function getDB(dynamoDb: DynamoDB.DocumentClient, pk: UserPK, sk: UserMatchSK): Promise<UserMatch[]> {
	if(pk.ServerId === null || pk.ServerId === undefined) {
		throw new Error("PK property: ServerId is not set.");
	}
	if(pk.UserId === null || pk.UserId === undefined) {
		throw new Error("PK property: UserId is not set.");
	}
	let isPartialSK = true;
	const pkKey = `#USER#${pk.ServerId}#${pk.UserId}`;
	let skKey = 'MATCH#';
	if(sk.MatchNumber !== null && sk.MatchNumber !== undefined) {
		skKey += `${sk.MatchNumber}`;
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

export async function getDBInverse(dynamoDb: DynamoDB.DocumentClient, pk: UserMatchSK, sk: UserPK): Promise<UserMatch[]> {
	if(pk.MatchNumber === null || pk.MatchNumber === undefined) {
		throw new Error("PK property: MatchNumber is not set.");
	}
	let isPartialSK = true;
	const pkKey = `MATCH#${pk.MatchNumber}`;
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

