// This file is auto-generated.

import * as DynamoDB from 'aws-sdk/clients/dynamodb';
import { Game } from "../models/Game";
import { GamePK } from "./models/GamePK";

export async function getDB(dynamoDb: DynamoDB.DocumentClient, pk: GamePK): Promise<Game[]> {
	if(pk.ServerId === null || pk.ServerId === undefined) {
		throw new Error("PK property: ServerId is not set.");
	}
	if(pk.GameCode === null || pk.GameCode === undefined) {
		throw new Error("PK property: GameCode is not set.");
	}
	let isPartialSK = true;
	const pkKey = `#GAME#${pk.ServerId}#${pk.GameCode}`;
	let skKey = '#GAME#';
	if(pk.ServerId !== null && pk.ServerId !== undefined) {
		skKey += `${pk.ServerId}`;
		if(pk.GameCode !== null && pk.GameCode !== undefined) {
			skKey += `#${pk.GameCode}`;
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

