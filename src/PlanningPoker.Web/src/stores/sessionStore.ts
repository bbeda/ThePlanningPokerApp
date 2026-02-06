import { defineStore } from "pinia";
import { ref, computed } from "vue";
import type { Session, User, VotingRound, Vote, VotingResults } from "@/types";

const STORAGE_KEYS = {
  CURRENT_USER: "planning-poker-current-user",
  SESSION_CODE: "planning-poker-session-code",
};

export const useSessionStore = defineStore("session", () => {
  // Cross-tab sync for own votes
  const crossTabChannel = new BroadcastChannel("planning-poker-sync");

  // State
  const session = ref<Session | null>(null);
  const currentUser = ref<User | null>(null);
  const isConnected = ref(false);

  // Getters
  const isOwner = computed(
    () => currentUser.value?.id === session.value?.ownerId,
  );

  const currentRound = computed(() => session.value?.currentRound);

  const hasVoted = computed(() => {
    if (!currentRound.value || !currentUser.value) return false;
    return currentRound.value.votes.some(
      (v) => v.userId === currentUser.value!.id,
    );
  });

  const currentVote = computed(() => {
    if (!currentRound.value || !currentUser.value) return null;
    return currentRound.value.votes.find(
      (v) => v.userId === currentUser.value!.id,
    );
  });

  const canReveal = computed(() => {
    return (
      isOwner.value &&
      currentRound.value?.status === "InProgress" &&
      (currentRound.value?.votes.length ?? 0) > 0
    );
  });

  const canReset = computed(() => {
    return isOwner.value && currentRound.value?.status === "Revealed";
  });

  const canStartVoting = computed(() => {
    return isOwner.value && !currentRound.value;
  });

  // Actions
  function setSession(newSession: Session) {
    session.value = newSession;
    localStorage.setItem(STORAGE_KEYS.SESSION_CODE, newSession.sessionCode);
  }

  function setCurrentUser(user: User) {
    currentUser.value = user;
    localStorage.setItem(STORAGE_KEYS.CURRENT_USER, JSON.stringify(user));
  }

  function addUser(user: User) {
    if (session.value) {
      const existingIndex = session.value.users.findIndex(
        (u) => u.id === user.id,
      );
      if (existingIndex === -1) {
        session.value.users.push(user);
      }
    }
  }

  function removeUser(userId: string) {
    if (session.value) {
      session.value.users = session.value.users.filter((u) => u.id !== userId);
    }
  }

  function updateUserConnectionStatus(userId: string, isConnected: boolean) {
    if (session.value) {
      const user = session.value.users.find((u) => u.id === userId);
      if (user) {
        user.isConnected = isConnected;
      }
    }
  }

  function startVoting(round: Partial<VotingRound>) {
    if (session.value) {
      session.value.currentRound = {
        id: round.id!,
        status: "InProgress",
        startedAt: round.startedAt!,
        revealedAt: null,
        votes: [],
        results: null,
      };
    }
  }

  function updateVote(vote: Vote, broadcast = true) {
    if (currentRound.value) {
      const index = currentRound.value.votes.findIndex(
        (v) => v.userId === vote.userId,
      );
      const isOwnVote = vote.userId === currentUser.value?.id;

      if (index >= 0) {
        // Update existing vote
        // Show value if: 1) it's revealed, 2) it's your own vote, or 3) vote already has a value
        const existingValue = currentRound.value.votes[index].value;
        currentRound.value.votes[index] = {
          ...vote,
          value:
            currentRound.value.status === "Revealed" ||
            isOwnVote ||
            existingValue !== null
              ? (vote.value ?? existingValue)
              : null,
        };
      } else {
        // Add new vote
        // Show value if it's revealed or it's your own vote
        currentRound.value.votes.push({
          ...vote,
          value:
            currentRound.value.status === "Revealed" || isOwnVote
              ? vote.value
              : null,
        });
      }

      // Broadcast own vote to other tabs
      if (broadcast && isOwnVote && vote.value !== null) {
        crossTabChannel.postMessage({ type: "vote-updated", vote });
      }
    }
  }

  // Listen for vote updates from other tabs
  crossTabChannel.onmessage = (event: MessageEvent) => {
    if (event.data.type === "vote-updated" && currentRound.value) {
      updateVote(event.data.vote, false);
    }
  };

  function revealVotes(data: { results: VotingResults; votes: Vote[] }) {
    if (currentRound.value) {
      currentRound.value.status = "Revealed";
      currentRound.value.revealedAt = new Date();
      currentRound.value.results = data.results;
      currentRound.value.votes = data.votes;
    }
  }

  function resetVotes() {
    if (session.value) {
      session.value.currentRound = null;
    }
  }

  function restoreFromStorage() {
    const userJson = localStorage.getItem(STORAGE_KEYS.CURRENT_USER);
    const code = localStorage.getItem(STORAGE_KEYS.SESSION_CODE);

    let user: User | null = null;
    if (userJson) {
      try {
        user = JSON.parse(userJson);
      } catch {
        user = null;
      }
    }

    return { user, sessionCode: code };
  }

  function clearStorage() {
    localStorage.removeItem(STORAGE_KEYS.CURRENT_USER);
    localStorage.removeItem(STORAGE_KEYS.SESSION_CODE);
  }

  function reset() {
    session.value = null;
    currentUser.value = null;
    isConnected.value = false;
    clearStorage();
  }

  return {
    // State
    session,
    currentUser,
    isConnected,
    // Getters
    isOwner,
    currentRound,
    hasVoted,
    currentVote,
    canReveal,
    canReset,
    canStartVoting,
    // Actions
    setSession,
    setCurrentUser,
    addUser,
    removeUser,
    updateUserConnectionStatus,
    startVoting,
    updateVote,
    revealVotes,
    resetVotes,
    restoreFromStorage,
    clearStorage,
    reset,
  };
});
