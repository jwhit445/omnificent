import { assert } from 'console';
import { User } from '../user/user';
import { calculatePoints } from './handler';

test('if team1 wins a match they gain mmr', () => {
  const initalSigma: number = 850;
  const initialMMR: number = 2000;

  const user1: User = new User();
  user1.RoCoMMR = initialMMR;
  user1.RoCoSigma = initalSigma;
  const user2: User = new User();
  user2.RoCoMMR = initialMMR;
  user2.RoCoSigma = initalSigma;
  const user3: User = new User();
  user3.RoCoMMR = initialMMR;
  user3.RoCoSigma = initalSigma;
  const user4: User = new User();
  user4.RoCoMMR = initialMMR;
  user4.RoCoSigma = initalSigma;

  const user5: User = new User();
  user5.RoCoMMR = initialMMR;
  user5.RoCoSigma = initalSigma;
  const user6: User = new User();
  user6.RoCoMMR = initialMMR;
  user6.RoCoSigma = initalSigma;
  const user7: User = new User();
  user7.RoCoMMR = initialMMR;
  user7.RoCoSigma = initalSigma;
  const user8: User = new User();
  user8.RoCoMMR = initialMMR;
  user8.RoCoSigma = initalSigma;

  const team1Users: User[] = [user1, user2, user3, user4];
  const team2Users: User[] = [user5, user6, user7, user8];
  calculatePoints(team1Users, team2Users);
  // console.log("team1 new MMR: " + team1Users[0].RoCoMMR + ", Sigma: " + team1Users[0].RoCoSigma);
  // console.log("team2 new MMR: " + team2Users[0].RoCoMMR + ", Sigma: " + team2Users[0].RoCoSigma);
  expect(team1Users[0].RoCoMMR > initialMMR).toBeTruthy();
  expect(team2Users[0].RoCoMMR < initialMMR).toBeTruthy();
});

test('if team2 wins a match they gain mmr', () => {
  const initalSigma: number = 850;
  const initialMMR: number = 2000;

  const user1: User = new User();
  user1.RoCoMMR = initialMMR;
  user1.RoCoSigma = initalSigma;
  const user2: User = new User();
  user2.RoCoMMR = initialMMR;
  user2.RoCoSigma = initalSigma;
  const user3: User = new User();
  user3.RoCoMMR = initialMMR;
  user3.RoCoSigma = initalSigma;
  const user4: User = new User();
  user4.RoCoMMR = initialMMR;
  user4.RoCoSigma = initalSigma;

  const user5: User = new User();
  user5.RoCoMMR = initialMMR;
  user5.RoCoSigma = initalSigma;
  const user6: User = new User();
  user6.RoCoMMR = initialMMR;
  user6.RoCoSigma = initalSigma;
  const user7: User = new User();
  user7.RoCoMMR = initialMMR;
  user7.RoCoSigma = initalSigma;
  const user8: User = new User();
  user8.RoCoMMR = initialMMR;
  user8.RoCoSigma = initalSigma;

  const team1Users: User[] = [user1, user2, user3, user4];
  const team2Users: User[] = [user5, user6, user7, user8];
  calculatePoints(team2Users, team1Users);
  // console.log("team1 new MMR: " + team1Users[0].RoCoMMR + ", Sigma: " + team1Users[0].RoCoSigma);
  // console.log("team2 new MMR: " + team2Users[0].RoCoMMR + ", Sigma: " + team2Users[0].RoCoSigma);
  expect(team1Users[0].RoCoMMR < initialMMR).toBeTruthy();
  expect(team2Users[0].RoCoMMR > initialMMR).toBeTruthy();
});