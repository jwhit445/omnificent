//This file is auto-generated.

import DynamoDB from 'aws-sdk/clients/dynamodb';
import { UserPK } from "./models/UserPK";
import { UserPlatformSK } from "./models/UserPlatformSK";
import { UserPlatformGSI1SK } from "./models/UserPlatformGSI1SK";

export async function getDB(dynamoDb: DynamoDB.DocumentClient, pk: UserPK, sk: UserPlatformSK): Promise<any> {
	if(pk.ServerId === null || pk.ServerId === undefined) {
		throw new Error("PK property: ServerId is not set.")
	}
	if(pk.UserId === null || pk.UserId === undefined) {
		throw new Error("PK property: UserId is not set.")
	}
	var isPartialSK = true;
	let pkKey = `#USER#${pk.ServerId}#${pk.UserId}`;
	var skKey = 'PLATFORM#';
	if(sk.PlatformCode !== null && sk.PlatformCode !== undefined) {
		skKey += `${sk.PlatformCode}#`
		if(sk.PlatformUsername !== null && sk.PlatformUsername !== undefined) {
			skKey += `${sk.PlatformUsername}`
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

export async function getDBInverse(dynamoDb: DynamoDB.DocumentClient, pk: UserPlatformSK, sk: UserPK): Promise<any> {
	if(pk.PlatformCode === null || pk.PlatformCode === undefined) {
		throw new Error("PK property: PlatformCode is not set.")
	}
	if(pk.PlatformUsername === null || pk.PlatformUsername === undefined) {
		throw new Error("PK property: PlatformUsername is not set.")
	}
	var isPartialSK = true;
	let pkKey = `PLATFORM#${pk.PlatformCode}#${pk.PlatformUsername}`;
	var skKey = '#USER#';
	if(sk.ServerId !== null && sk.ServerId !== undefined) {
		skKey += `${sk.ServerId}#`
		if(sk.UserId !== null && sk.UserId !== undefined) {
			skKey += `${sk.UserId}`
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

export async function getDBGSI1(dynamoDb: DynamoDB.DocumentClient, pk: UserPK, sk: UserPlatformGSI1SK): Promise<any> {
	if(pk.ServerId === null || pk.ServerId === undefined) {
		throw new Error("PK property: ServerId is not set.")
	}
	if(pk.UserId === null || pk.UserId === undefined) {
		throw new Error("PK property: UserId is not set.")
	}
	var isPartialSK = true;
	let pkKey = `#USER#${pk.ServerId}#${pk.UserId}`;
	var skKey = 'PLATFORM#';
	if(sk.PlatformCode !== null && sk.PlatformCode !== undefined) {
		skKey += `${sk.PlatformCode}#`
		if(sk.PlatformId !== null && sk.PlatformId !== undefined) {
			skKey += `${sk.PlatformId}`
			isPartialSK = false;
		}
	}
	const params: any = {
		TableName: process.env.DYNAMODB_TABLE,
		IndexName: 'GSI1',
		KeyConditionExpression: `PK = :pkKey AND ${isPartialSK ? "begins_with(GSI1SK, :skKey)" : "GSI1SK = :skKey"}`,
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

