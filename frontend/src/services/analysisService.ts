import axios from 'axios';
import { BACKEND_URL } from '../config';

const ANALYSIS_API_URL = `${BACKEND_URL}/api/analysis`;

interface AnalysisResponse {
  recognizedSign: string;
  confidence: number;
}

export const predictSign = (landmarks: number[]): Promise<AnalysisResponse> =>
  axios
    .post<AnalysisResponse>(ANALYSIS_API_URL, { landmarks })
    .then((res) => res.data);