import { Handler, Context } from 'aws-lambda';
import { DynamoDB } from 'aws-sdk'
import { v4 } from 'uuid';
import { Team, team_to_ddb, team_to_ddb_update_params } from "./team";

const dynamoDb = new DynamoDB.DocumentClient()

export const register = async (event: any, context: Context): Promise<any> => {
    const data = JSON.parse(event.body)
    try {
        if (!validateTeam(data)) {
            throw new Error("Invalid request data");
        }
    } catch (error) {
        throw new Error("Invalid request data");
    }

    const params = {
        TableName: process.env.DYNAMODB_TABLE,
        Item: team_to_ddb(data),
    }

    // write the match to the database
    try {
        await dynamoDb.put(params).promise();
        const response = {
            statusCode: 200,
            body: JSON.stringify(params.Item)
        }
        return response;
    } catch (error) {
        throw new Error('Couldn\'t create the team for id:' + params.Item.Id)   
    }
};

export const getOne = async (event: any, context: Context): Promise<any> => {
    const params = {
        TableName: process.env.DYNAMODB_TABLE,
        Key: {
          Id: event.pathParameters.id,
          EntityType: 'team'
        },
    };
    
    // fetch user from the database by id
    try {
        const result = await dynamoDb.get(params).promise();
        const response = {
            statusCode: 200,
            body: JSON.stringify(result.Item),
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t fetch the team.',
        };
    }
};

export const update: Handler = (event: any, context: Context, callback: any) => {
    const data = JSON.parse(event.body)
    try {
        if (!validateTeam(data)) {
            console.error('Validation Failed')
            callback(new Error("Invalid request data"));
            return;
        }
    } catch(error) {
        callback(new Error("Invalid request data"));
        return;
    }

    const params = team_to_ddb_update_params(event.pathParameters.id, data);

    // write the team changes to the database
    dynamoDb.update(params, (error, result) => {
        // handle potential errors
        if (error) {
            console.error(error);
            callback(new Error('Couldn\'t update the team for id:' + data.Id + "Error: " + error));
            return;
        }

        // create a response
        const response = {
            statusCode: 200,
            body: "Team updated successfully"
        }
        callback(null, response);
    })
};

export const getStats: Handler = (event: any, context: Context, callback: any) => {
    callback(null, {
        statusCode: 200,
        body: "Successful getStats!"
    });
};

function validateTeam(team: Team): boolean {
    try {
        return team.Id !== undefined && team.Id !== null
        && team.TeamName !== undefined && team.TeamName !== null
        && team.GameName !== undefined && team.GameName !== null
        && team.CaptainId !== undefined && team.CaptainId !== null
        && team.MemberIds !== undefined && team.MemberIds !== null;
    } catch (error) {
        return false;
    }
}