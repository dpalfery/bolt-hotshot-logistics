# Demo-full-prompt.md

Dashboard update

## Steps

Update the existing Hotshot Logistics Admin Dashboard UI to include five new job status cards, following the same design system for layout, spacing, and typography.

Current dashboard metrics:

* Total Jobs
* Active Jobs
* Active Drivers
* Overdue Invoices

New Component:
- Render the new component below the “Recent Activity” section in the Admin Dashboard
- Update the database seed to include varied data for each status
- Ensure backend services support the new status fields end-to-end

Add the following status cards below the main metrics row (same style, smaller size):

* Pending – Color: #3B82F6 (Blue) – Icon: Clock
* Assigned – Color: #0EA5E9 (Cyan) – Icon: ClipboardList
* EnRoute – Color: #0284C7 (Teal Blue) – Icon: Truck
* InProgress – Color: #0369A1 (Azure Blue) – Icon: Settings
* InTransit – Color: #0F766E (Green-Teal) – Icon: Package


Each card should:

* Show the icon on the left of the status label
* Place the label and icon above the number
* Display the count as a large bold number centered below
* Use the same padding, shadows, and rounded corners as the existing metric cards
* Follow the Inter font with semi-bold labels and bold counts
* Use a responsive grid layout (Tailwind: grid grid-cols-5 gap-4)

Data:

* Seed data counts should appear random and realistic, not sequential (0,1,2,3,4)
* Prepare for live updates later via SignalR


