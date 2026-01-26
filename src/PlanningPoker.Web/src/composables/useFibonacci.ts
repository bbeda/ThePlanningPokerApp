const FIBONACCI_VALUES = [1, 2, 3, 5, 8, 13, 21] as const

export function useFibonacci() {
  function isValidValue(value: number): boolean {
    return FIBONACCI_VALUES.includes(value as any)
  }

  function getValues() {
    return [...FIBONACCI_VALUES]
  }

  return {
    values: FIBONACCI_VALUES,
    isValidValue,
    getValues
  }
}
