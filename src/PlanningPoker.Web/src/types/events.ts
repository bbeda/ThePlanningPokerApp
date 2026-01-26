export type SseEventType =
  | 'user_joined'
  | 'user_left'
  | 'voting_started'
  | 'vote_submitted'
  | 'votes_revealed'
  | 'votes_reset'
  | 'session_closed'

export interface SseEvent {
  type: SseEventType
  data: any
}
