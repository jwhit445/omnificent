import * as chai from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';

chai.use(sinonChai);
const expect = chai.expect;

import { decode, encode } from '../src/utils/base64Helper';

describe('authHeader tests', () => {
  let sandbox: sinon.SinonSandbox = sinon.createSandbox();

  beforeEach(() => {
    // stub methods to spy on. this allows you to call spy.should.have.been.called
    // spy = sandbox.spy()
  });

  afterEach(() => {
    sandbox.restore();
  });

  it('Successfully decodes an encoded string', async () => {
    // Arrange
    // base64 encoded: 'teststring'
    const encodedStr: string = "dGVzdHN0cmluZw==";
    const decodedStrValue: string = "teststring";
    // Act
    const decodedStr: string = decode(encodedStr);
    // Assert
    expect(decodedStr).to.equal(decodedStrValue);
  });

  it('Successfully encodes a string', async () => {
    // Arrange
    // base64 encoded: 'teststring'
    const str: string = "teststring";
    const encodedStrVal: string = "dGVzdHN0cmluZw==";
    // Act
    const encodedStr: string = encode(str);
    // Assert
    expect(encodedStr).to.equal(encodedStrVal);
  });
});