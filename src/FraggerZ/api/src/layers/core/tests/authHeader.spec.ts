import * as chai from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';

chai.use(sinonChai);
const expect = chai.expect;

import { BasicAuth, getBasicAuth } from '../src/utils/authHeader';

describe('authHeader tests', () => {
  let sandbox: sinon.SinonSandbox = sinon.createSandbox();

  beforeEach(() => {
    // stub methods to spy on. this allows you to call spy.should.have.been.called
    // spy = sandbox.spy()
  });

  afterEach(() => {
    sandbox.restore();
  });

  it('Successfully retrieves a valid basic auth header', async () => {
    // Arrange
    // hardcoded basic auth header with testusername:testpassword
    const basicAuthHeader: string = "Basic dGVzdHVzZXJuYW1lOnRlc3RwYXNzd29yZA==";
    const username: string = "testusername";
    const password: string = "testpassword";
    // Act
    const basicAuth: BasicAuth = getBasicAuth(basicAuthHeader);
    // Assert
    expect(basicAuth.username).to.equal(username);
    expect(basicAuth.password).to.equal(password);
  });

  it('Throws error for invalid basic auth header', async () => {
    // Arrange
    // hardcoded basic auth header with testusername:testpassword
    const basicAuthHeader: string = "Basic zZXJuYW1lOnRlc3RwYXNz";
    const username: string = "testusername";
    const password: string = "testpassword";
    // Act & Assert
    expect(() => getBasicAuth(basicAuthHeader)).to.throw('');
  });
});