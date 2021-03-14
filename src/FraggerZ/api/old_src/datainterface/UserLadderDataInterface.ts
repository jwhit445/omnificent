// This file is auto-generated.

import DynamoDB from 'aws-sdk/clients/dynamodb';
import { UserLadder } from "../models/UserLadder";
import { UserPK } from "./models/UserPK";
import { UserLadderSK } from "./models/UserLadderSK";

export async function getDB(dynamoDb: DynamoDB.DocumentClient, pk: UserPK, sk: UserLadderSK): Promise<UserLadder[]> {
	if(pk.ServerId === null || pk.ServerId === undefined) {
		throw new Error("PK property: ServerId is not set.");
	}
	if(pk.UserId === null || pk.UserId === undefined) {
		throw new Error("PK property: UserId is not set.");
	}
	let isPartialSK = true;
	const pkKey = `#USER#${pk.ServerId}#${pk.UserId}`;
	let skKey = 'LADDER#';
	if(sk.GameCode !== null && sk.GameCode !== undefined) {
		skKey += `${sk.GameCode}`;
		if(sk.LadderName !== null && sk.LadderName !== undefined) {
			skKey += `#${sk.LadderName}`;
			if(sk.SeasonId !== null && sk.SeasonId !== undefined) {
				skKey += `#${sk.SeasonId}`;
				isPartialSK = false;
			}
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

export async function getDBInverse(dynamoDb: DynamoDB.DocumentClient, pk: UserLadderSK, sk: UserPK): Promise<UserLadder[]> {
	if(pk.GameCode === null || pk.GameCode === undefined) {
		throw new Error("PK property: GameCode is not set.");
	}
	if(pk.LadderName === null || pk.LadderName === undefined) {
		throw new Error("PK property: LadderName is not set.");
	}
	if(pk.SeasonId === null || pk.SeasonId === undefined) {
		throw new Error("PK property: SeasonId is not set.");
	}
	let isPartialSK = true;
	const pkKey = `LADDER#${pk.GameCode}#${pk.LadderName}#${pk.SeasonId}`;
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

