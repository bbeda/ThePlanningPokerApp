import { ref } from "vue";
import type { Session, User, VotingRound, VotingResults } from "@/types";
import { getBrowserId } from "@/utils/browserIdentity";

const API_BASE_URL = "/api";

export function useApi() {
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function request<T>(
    endpoint: string,
    options?: RequestInit,
  ): Promise<T> {
    loading.value = true;
    error.value = null;

    try {
      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        ...options,
        headers: {
          "Content-Type": "application/json",
          ...options?.headers,
        },
      });

      if (!response.ok) {
        const errorData = await response
          .json()
          .catch(() => ({ error: response.statusText }));
        throw new Error(errorData.error || `HTTP ${response.status}`);
      }

      if (response.status === 204) {
        return null as T;
      }

      return await response.json();
    } catch (err) {
      error.value = err instanceof Error ? err.message : "Unknown error";
      throw err;
    } finally {
      loading.value = false;
    }
  }

  // Session APIs
  async function createSession(ownerName: string) {
    return request<Session>("/sessions", {
      method: "POST",
      body: JSON.stringify({
        ownerName,
        browserId: getBrowserId(),
      }),
    });
  }

  async function getSession(sessionCode: string) {
    return request<Session>(`/sessions/${sessionCode}`);
  }

  async function joinSession(sessionCode: string, userName: string) {
    return request<User>(`/sessions/${sessionCode}/users`, {
      method: "POST",
      body: JSON.stringify({
        sessionCode,
        userName,
        browserId: getBrowserId(),
      }),
    });
  }

  async function leaveSession(sessionCode: string, userId: string) {
    return request(`/sessions/${sessionCode}/users/${userId}`, {
      method: "DELETE",
    });
  }

  // Voting APIs
  async function startVoting(sessionCode: string, userId: string) {
    return request<VotingRound>(
      `/sessions/${sessionCode}/voting/start?userId=${userId}`,
      { method: "POST" },
    );
  }

  async function submitVote(
    sessionCode: string,
    userId: string,
    value: number,
  ) {
    return request<any>(
      `/sessions/${sessionCode}/voting/votes?userId=${userId}`,
      {
        method: "POST",
        body: JSON.stringify({ value }),
      },
    );
  }

  async function revealVotes(sessionCode: string, userId: string) {
    return request<VotingResults>(
      `/sessions/${sessionCode}/voting/reveal?userId=${userId}`,
      { method: "POST" },
    );
  }

  async function resetVotes(sessionCode: string, userId: string) {
    return request(`/sessions/${sessionCode}/voting/reset?userId=${userId}`, {
      method: "POST",
    });
  }

  async function closeSession(sessionCode: string, userId: string) {
    return request(`/sessions/${sessionCode}?userId=${userId}`, {
      method: "DELETE",
    });
  }

  return {
    loading,
    error,
    createSession,
    getSession,
    joinSession,
    leaveSession,
    startVoting,
    submitVote,
    revealVotes,
    resetVotes,
    closeSession,
  };
}
