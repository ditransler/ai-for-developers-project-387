import type { components } from '~/types/api.generated'

type EventType = components['schemas']['EventType']
type TimeSlot = components['schemas']['TimeSlot']
type Booking = components['schemas']['Booking']
type CreateBookingRequest = components['schemas']['CreateBookingRequest']
type EventTypeCreate = components['schemas']['EventTypeCreate']
type EventTypeUpdate = components['schemas']['EventTypeUpdate']

export function useBookingApi() {
  const api = useApiClient()

  return {
    listPublicEventTypes: () => api<EventType[]>('/public/event-types'),

    listAvailableSlots: (eventTypeId: string) =>
      api<TimeSlot[]>(`/public/event-types/${eventTypeId}/available-slots`),

    createBooking: (body: CreateBookingRequest) =>
      api<Booking>('/public/bookings', { method: 'POST', body }),

    listAdminBookings: () => api<Booking[]>('/admin/bookings'),

    listAdminEventTypes: () => api<EventType[]>('/admin/event-types'),

    createEventType: (body: EventTypeCreate) =>
      api<EventType>('/admin/event-types', { method: 'POST', body }),

    updateEventType: (eventTypeId: string, body: EventTypeUpdate) =>
      api<EventType>(`/admin/event-types/${eventTypeId}`, {
        method: 'PATCH',
        body,
      }),

    deleteEventType: (eventTypeId: string) =>
      api<undefined>(`/admin/event-types/${eventTypeId}`, { method: 'DELETE' }),
  }
}
