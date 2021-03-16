import { get, post } from 'axios';
import { expect } from 'chai';
// import { v4 as uuidv4 } from 'uuid';

const region = process.env.AWS_REGION;
const cognitoServiceProvider = new AWS.CognitoIdentityServiceProvider({
  apiVersion: '2016-04-18',
  region
});

describe('End-to-end tests for FraggerZ API', () => {
  const apiEndpoint = process.env.API_ENDPOINT;
  const userPoolId = process.env.USER_POOL_ID;
  const userPoolClientId = process.env.USER_POOL_CLIENT_ID;
  // const password = uuidv4();

  before(async () => {
  });

  after(async () => {

  });

  context('Registering a user', () => {

    it('should not required authentication', async () => {
    });

    it('should not return any information', async () => {
    });
  });

  context('Getting a user', () => {

    it('should return entire user', async () => {
    });
  
    it('should return an error if user does not exist', async () => {
    });
  });
});