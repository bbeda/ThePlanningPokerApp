import { ref, onUnmounted } from "vue";
import { useSessionStore } from "@/stores/sessionStore";
import { useRouter } from "vue-router";

export function useSse() {
  const sessionStore = useSessionStore();
  const router = useRouter();
  const eventSource = ref<EventSource | null>(null);
  const isConnected = ref(false);
  const reconnectAttempts = ref(0);
  const maxReconnectAttempts = 5;

  function connect(sessionCode: string, userId: string) {
    disconnect();

    const url = `/api/sessions/${sessionCode}/events?userId=${userId}`;
    eventSource.value = new EventSource(url);

    eventSource.value.onopen = () => {
      isConnected.value = true;
      reconnectAttempts.value = 0;
      sessionStore.isConnected = true;
      // Update current user's connection status in the users list
      sessionStore.updateUserConnectionStatus(userId, true);
    };

    eventSource.value.onerror = () => {
      isConnected.value = false;
      sessionStore.isConnected = false;
      // Update current user's connection status in the users list
      sessionStore.updateUserConnectionStatus(userId, false);

      if (reconnectAttempts.value < maxReconnectAttempts) {
        reconnectAttempts.value++;
        const delay = 2000 * reconnectAttempts.value;
        setTimeout(() => {
          if (reconnectAttempts.value <= maxReconnectAttempts) {
            connect(sessionCode, userId);
          }
        }, delay);
      }
    };

    // Register event handlers
    eventSource.value.addEventListener("user_joined", handleUserJoined);
    eventSource.value.addEventListener("user_left", handleUserLeft);
    eventSource.value.addEventListener("user_connected", handleUserConnected);
    eventSource.value.addEventListener(
      "user_disconnected",
      handleUserDisconnected,
    );
    eventSource.value.addEventListener("voting_started", handleVotingStarted);
    eventSource.value.addEventListener("vote_submitted", handleVoteSubmitted);
    eventSource.value.addEventListener("votes_revealed", handleVotesRevealed);
    eventSource.value.addEventListener("votes_reset", handleVotesReset);
    eventSource.value.addEventListener("session_closed", handleSessionClosed);
  }

  function disconnect() {
    if (eventSource.value) {
      eventSource.value.close();
      eventSource.value = null;
      isConnected.value = false;
      sessionStore.isConnected = false;
    }
  }

  function handleUserJoined(event: MessageEvent) {
    const data = JSON.parse(event.data);
    sessionStore.addUser({
      id: data.id,
      name: data.name,
      isOwner: data.isOwner,
      joinedAt: new Date(data.joinedAt),
      isConnected: data.isConnected ?? false,
    });
  }

  function handleUserLeft(event: MessageEvent) {
    const data = JSON.parse(event.data);
    sessionStore.removeUser(data.userId);
  }

  function handleUserConnected(event: MessageEvent) {
    const data = JSON.parse(event.data);
    sessionStore.updateUserConnectionStatus(data.userId, true);
  }

  function handleUserDisconnected(event: MessageEvent) {
    const data = JSON.parse(event.data);
    sessionStore.updateUserConnectionStatus(data.userId, false);
  }

  function handleVotingStarted(event: MessageEvent) {
    const data = JSON.parse(event.data);
    sessionStore.startVoting({
      id: data.id,
      startedAt: new Date(data.startedAt),
    });
  }

  function handleVoteSubmitted(event: MessageEvent) {
    const data = JSON.parse(event.data);
    // Don't reveal the value until votes are revealed
    sessionStore.updateVote({
      userId: data.userId,
      userName: data.userName,
      value: null,
      submittedAt: new Date(),
    });
  }

  function handleVotesRevealed(event: MessageEvent) {
    const data = JSON.parse(event.data);
    sessionStore.revealVotes({
      results: data.results,
      votes: data.votes.map((v: any) => ({
        ...v,
        submittedAt: new Date(v.submittedAt),
      })),
    });
  }

  function handleVotesReset() {
    sessionStore.resetVotes();
  }

  function handleSessionClosed() {
    disconnect();
    alert("The session has been closed by the owner");
    router.push("/");
  }

  onUnmounted(() => {
    disconnect();
  });

  return {
    isConnected,
    connect,
    disconnect,
  };
}
