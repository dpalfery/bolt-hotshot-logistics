# Demo-full-prompt.md

Dashboard update

## Steps

Update the existing Hotshot Logistics Admin Dashboard UI to include five new job status cards, keeping the same design system (card layout, spacing, and typography).

Base: Current dashboard displays “Total Jobs,” “Active Jobs,” “Active Drivers,” and “Overdue Invoices.”

Add the following status cards below the main metrics row (same style, smaller size):

Pending (0) – Color: #3B82F6 (Blue)

Assigned (1) – Color: #0EA5E9 (Cyan)

EnRoute (2) – Color: #0284C7 (Teal Blue)

InProgress (3) – Color: #0369A1 (Azure Blue)

InTransit (4) – Color: #0F766E (Green-Teal)

Each card should display:

Top label (e.g., “Pending”)

A rounded square badge with a single letter icon (P, A, E, I, I)

The count (large bold number) below

Layout guidelines:

Cards use the same padding, shadow, and rounded corners as the existing four cards.

Use responsive grid layout (Tailwind grid grid-cols-5 gap-4 under the main metrics row).

Maintain consistent typography: Inter, semi-bold for labels, bold for counts.

Place this new section right below the “Recent Activity” area.

Code targets:

Frontend built in React + Tailwind (Next.js 15 App Router).

Create a new React component JobStatusCards.tsx that receives props:

interface JobStatus {
  label: string;
  count: number;
  color: string;
  icon: string;
}


Import and render it inside the Admin Dashboard page.

Prepare props for live updates from the backend once SignalR feed is added.