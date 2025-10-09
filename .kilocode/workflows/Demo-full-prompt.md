# Demo-full-prompt.md

Dashboard update

## Steps

Update the existing Hotshot Logistics Admin Dashboard UI to include 4 new job status cards, following the same design system for layout, spacing, and typography.

Current dashboard metrics:

* Total Jobs
* Active Jobs
* Active Drivers
* Overdue Invoices

New Component:
- Render the new component above the “Recent Activity”, between the main status cubes and the "Recent Activity" section in the Admin Dashboard
- Ensure backend services support the new status fields end-to-end

Add the following status cards below the main metrics row (same style, smaller size):

Pending – Color: #3B82F6 (Blue) – Icon: Hourglass (
Assigned – Color: #0EA5E9 (Cyan) – Icon: ClipboardList
EnRoute – Color: #0284C7 (Teal Blue) – Icon: Truck
Received – Color: #0369A1 (Azure Blue) – Icon: Inbox

Each card should:

* Use the same padding, shadows, and rounded corners as the existing metric cards
* Follow the Inter font with semi-bold labels and bold counts
* Use a responsive grid layout (Tailwind: grid grid-cols-5 gap-4)
