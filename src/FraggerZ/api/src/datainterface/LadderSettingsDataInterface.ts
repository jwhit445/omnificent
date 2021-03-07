//This file is auto-generated.

import DynamoDB from 'aws-sdk/clients/dynamodb';
import { LadderPK } from "./models/LadderPK";
import { LadderSettingsSK } from "./models/LadderSettingsSK";

export async function getDB(dynamoDb: DynamoDB.DocumentClient, pk: LadderPK, sk: LadderSettingsSK): Promise<any> {
	if(pk.ServerId === null || pk.ServerId === undefined) {
		throw new Error("PK property: ServerId is not set.")
	}
	if(pk.GameCode === null || pk.GameCode === undefined) {
		throw new Error("PK property: GameCode is not set.")
	}
	if(pk.LadderName === null || pk.LadderName === undefined) {
		throw new Error("PK property: LadderName is not set.")
	}
	var isPartialSK = true;
	let pkKey = `#LADDER#${pk.ServerId}#${pk.GameCode}#${pk.LadderName}`;
	var skKey = 'SETTINGS#';
	if(sk.SeasonId !== null && sk.SeasonId !== undefined) {
		skKey += `${sk.SeasonId}`
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

export async function getDBInverse(dynamoDb: DynamoDB.DocumentClient, pk: LadderSettingsSK, sk: LadderPK): Promise<any> {
	if(pk.SeasonId === null || pk.SeasonId === undefined) {
		throw new Error("PK property: SeasonId is not set.")
	}
	var isPartialSK = true;
	let pkKey = `SETTINGS#${pk.SeasonId}`;
	var skKey = '#LADDER#';
	if(sk.ServerId !== null && sk.ServerId !== undefined) {
		skKey += `${sk.ServerId}#`
		if(sk.GameCode !== null && sk.GameCode !== undefined) {
			skKey += `${sk.GameCode}#`
			if(sk.LadderName !== null && sk.LadderName !== undefined) {
				skKey += `${sk.LadderName}`
				isPartialSK = false;
			}
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

