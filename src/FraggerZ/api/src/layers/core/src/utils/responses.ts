import { APIGatewayProxyResult } from 'aws-lambda';

const HTTP_OK = 200;
const HTTP_BAD_REQUEST = 400;
const HTTP_SERVER_ERROR = 500;

export function isClientError(statusCode: number): boolean {
  return statusCode >= 400 && statusCode <= 499;
}

function response(statusCode: number, body: object, headers: any = {}): APIGatewayProxyResult {
  return {
    statusCode,
    headers: {
      'Access-Control-Allow-Origin': '*',
      ...headers,
    },
    body: JSON.stringify(body),
  }
}

export function responseOk(body: object, headers: any = {}): APIGatewayProxyResult {
  return response(HTTP_OK, body, headers);
}

export function responseBadRequest(body: object, headers: any = {}): APIGatewayProxyResult {
  return response(HTTP_BAD_REQUEST, body, headers);
}

export function responseServerError(body: object, headers: any = {}): APIGatewayProxyResult {
  return response(HTTP_SERVER_ERROR, body, headers);
}