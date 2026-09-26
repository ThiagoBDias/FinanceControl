const eventName = "financecontrol:data-changed";

export function notifyFinanceDataChanged() {
  window.dispatchEvent(new Event(eventName));
}

export function onFinanceDataChanged(callback: () => void) {
  window.addEventListener(eventName, callback);
  return () => window.removeEventListener(eventName, callback);
}
