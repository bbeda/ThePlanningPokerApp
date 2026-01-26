export function getBrowserId(): string {
  const key = 'planning-poker-browser-id'
  let browserId = localStorage.getItem(key)

  if (!browserId) {
    browserId = crypto.randomUUID()
    localStorage.setItem(key, browserId)
  }

  return browserId
}

export function clearBrowserId(): void {
  localStorage.removeItem('planning-poker-browser-id')
}
