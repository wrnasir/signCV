import axios from 'axios';
import { BACKEND_URL } from '../config';

const CHALLENGE_API_URL = `${BACKEND_URL}/api/challenge`;

interface ChallengeRequest {
  skillLevel: number;
  masteredSigns: string[];
  streak: number;
  usedWords: string[];
}

interface ChallengeResponse {
  targetWord: string;
  hint: string;
  difficulty: string;
}

export const generateChallenge = (request: ChallengeRequest): Promise<ChallengeResponse> =>
  axios
    .post<ChallengeResponse>(`${CHALLENGE_API_URL}/generate`, request)
    .then((res) => res.data);