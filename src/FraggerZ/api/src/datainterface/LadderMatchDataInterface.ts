// This file is auto-generated.

import DynamoDB from 'aws-sdk/clients/dynamodb';
import { LadderPK } from "./models/LadderPK";
import { LadderMatchSK } from "./models/LadderMatchSK";
import { LadderMatchGSI1SK } from "./models/LadderMatchGSI1SK";

export async function getDB(dynamoDb: DynamoDB.DocumentClient, pk: LadderPK, sk: LadderMatchSK): Promise<any> {
	if(pk.ServerId === null || pk.ServerId === undefined) {
		throw new Error("PK property: ServerId is not set.")
	}
	if(pk.GameCode === null || pk.GameCode === undefined) {
		throw new Error("PK property: GameCode is not set.")
	}
	if(pk.LadderName === null || pk.LadderName === undefined) {
		throw new Error("PK property: LadderName is not set.")
	}
	let isPartialSK = true;
	const pkKey = `#LADDER#${pk.ServerId}#${pk.GameCode}#${pk.LadderName}`;
	let skKey = 'MATCH#';
	if(sk.MatchNumber !== null && sk.MatchNumber !== undefined) {
		skKey += `${sk.MatchNumber}`
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

export async function getDBInverse(dynamoDb: DynamoDB.DocumentClient, pk: LadderMatchSK, sk: LadderPK): Promise<any> {
	if(pk.MatchNumber === null || pk.MatchNumber === undefined) {
		throw new Error("PK property: MatchNumber is not set.")
	}
	let isPartialSK = true;
	const pkKey = `MATCH#${pk.MatchNumber}`;
	let skKey = '#LADDER#';
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

export async function getDBGSI1(dynamoDb: DynamoDB.DocumentClient, pk: LadderPK, sk: LadderMatchGSI1SK): Promise<any> {
	if(pk.ServerId === null || pk.ServerId === undefined) {
		throw new Error("PK property: ServerId is not set.")
	}
	if(pk.GameCode === null || pk.GameCode === undefined) {
		throw new Error("PK property: GameCode is not set.")
	}
	if(pk.LadderName === null || pk.LadderName === undefined) {
		throw new Error("PK property: LadderName is not set.")
	}
	let isPartialSK = true;
	const pkKey = `#LADDER#${pk.ServerId}#${pk.GameCode}#${pk.LadderName}`;
	let skKey = 'SEASON#';
	if(sk.SeasonId !== null && sk.SeasonId !== undefined) {
		skKey += `${sk.SeasonId}#MATCH#`
		if(sk.MatchNumber !== null && sk.MatchNumber !== undefined) {
			skKey += `${sk.MatchNumber}`
			isPartialSK = false;
		}
	}
	const params: any = {
		TableName: process.env.DYNAMODB_TABLE,
		IndexName: 'GSI1',
		KeyConditionExpression: `PK = :pkKey AND ${isPartialSK ? "begins_with(GS1SK, :skKey)" : "GS1SK = :skKey"}`,
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

