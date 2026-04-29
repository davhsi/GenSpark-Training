**Busly — Online Bus Ticket Booking Platform**


## **Software Requirements Specification (SRS)**

**Version:** 1.0\
**Status:** Draft\
**Last Updated:** Apr 23, 2026\
**Prepared By:** [`Davish E`](mailto:davish.std@gmail.com)


# **1. Project Overview**

Busly is a modern online bus ticket booking platform designed to bridge the gap between bus operators and travelers. The system utilizes an Angular-based frontend and a .NET Web API backend with a PostgreSQL database. It features a robust multi-role architecture supporting Passengers, Bus Operators, and a Platform Administrator.


# **2. User Roles & Access Control**

## **2.1 Passenger (End User)**

Passengers are the primary consumers of the platform. They can browse services anonymously but must authenticate for transactions.

- **Discovery:** Search for buses by source, destination, and date using a fuzzy autocomplete location search.

- **Booking Flow:** Select seats from an interactive layout. Seats are color-coded by gender (Blue for Male, Pink for Female).

- **Authentication:** Triggered only upon booking. Requires Name, Age, and Gender.

- **Transaction:** Temporary seat locking during a payment grace period; receipt of PDF tickets via SMTP email.

- **Self-Service:** Access to a booking history and cancellation requests.


## **2.2 Bus Operator**

Operators manage the fleet and inventory. Access is restricted until Admin approval is granted.

- **Inventory Management:** Register buses (number plate, name, owner) and upload unique seat layouts.

- **Route Assignment:** Bind buses to existing Admin-defined routes.

- **Logistics:** Define specific boarding and alighting points based on head office locations in each city.

- **Operations:** Set ticket pricing and monitor all associated bookings.

- **Maintenance:** Ability to temporarily disable or permanently remove buses from the service list.


## **2.3 Admin (Platform Superuser)**

The Admin manages the ecosystem and financial health of the platform.

- **Route Management:** Exclusive rights to create and manage the city-pair route network (e.g., Coimbatore → Bangalore).

- **Quality Control:** Approve or reject both Bus Operator registrations and individual bus submissions.

- **Account Governance:** Enable or disable operator accounts based on compliance or performance.

- **Financial Oversight:** View aggregate platform revenue, specifically tracking convenience fees collected per booking.

- **Policy Management:** Admin can publish new versions of Terms & Conditions, which automatically triggers mandatory user re-acceptance.


# **3. Module Breakdown**

| ID       | Module Name        | Description                                                                                                                                                                                                                                                               |
| -------- | ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **M-01** | Auth & Onboarding  | Role-aware registration with JWT tokens and Admin approval queues for operators. Includes a mandatory T\&C acceptance and compliance check (requiring re-acceptance on version updates) before any booking transaction, with the accepted version and timestamp recorded. |
| **M-02** | Route Management   | Admin-only CRUD for city pairs; source for all operator bus listings.                                                                                                                                                                                                     |
| **M-03** | Bus & Layout       | Operator registration of vehicles and seat configuration; requires Admin validation.                                                                                                                                                                                      |
| **M-04** | Search & Discovery | Public fuzzy search and seat availability view; no login required for browsing.                                                                                                                                                                                           |
| **M-05** | Seat Selection     | Interactive map with gender-based coloring and optimistic DB locking (10-min grace).                                                                                                                                                                                      |
| **M-06** | Booking & Payment  | Dummy payment integration; handles convenience fee calculation and PDF generation.                                                                                                                                                                                        |
| **M-07** | Notifications      | SMTP-driven booking confirmations, cancellation alerts, and journey reminders.                                                                                                                                                                                            |
| **M-08** | Cancellation       | Logic for tiered refunds and automated coupon issuance for operator-led cancellations.                                                                                                                                                                                    |
| **M-09** | Operator Dashboard | Real-time tracking of bookings, bus status, and boarding point management.                                                                                                                                                                                                |
| **M-10** | Admin Dashboard    | High-level analytics on revenue, approval queues, and operator oversight.                                                                                                                                                                                                 |


# **4. Business Rules: Cancellation & Refunds**

The following rules govern the refund process and are calculated based on the time remaining until the scheduled departure.

| Scenario                | Trigger    | Refund Amount | Additional Action                   |
| ----------------------- | ---------- | ------------- | ----------------------------------- |
| Cancellation > 24 hrs   |  Passenger | 85%           |  Credit to original payment method  |
| Cancellation 12–24 hrs  |  Passenger | 50%           |  Credit to original payment method  |
| Cancellation < 12 hrs   |  Passenger | 0%            |  No refund provided                 |
| Bus removal/disablement |  Operator  | 100%          |  Full refund + Coupon code via SMTP |


# **5. Technical Stack & Non-Functional Requirements**

## **5.1 Tech Stack**

- **Frontend:** Angular + Bootstrap / Angular Material.

- **Backend:** .NET Web API (C#).

- **Database:** PostgreSQL.

- **Authentication:** JWT (JSON Web Tokens) with Role-Based Access Control (RBAC).


## **5.2 Non-Functional Requirements**

- **Security:** Role-based route guards in Angular; password hashing on the backend. JWT tokens are stored in `HttpOnly` cookies set by the server — never in localStorage or sessionStorage — to prevent XSS-based token theft.

- **Concurrency:** Optimistic locking in the database to handle simultaneous seat selection during the 10-minute payment grace period.

- **Communication:** Asynchronous SMTP queue for email dispatch to ensure UI responsiveness.

- **Revenue Model:** Configurable platform convenience fee (flat or percentage-based) tracked in the Admin revenue module.

- **Portability:** Use of environment variables (.env) for system configuration (SMTP, DB Strings).


### **Legal Compliance**

- The system stores Terms & Conditions acceptance metadata including version and timestamp.

- Historical acceptance records must be retained for audit and dispute resolution.


# **6. System Consistency and Reliability**

## **6.1 Transactional Integrity**

- **Payment Consistency:** A booking is considered CONFIRMED only after successful payment confirmation. Failed or timed-out payments must automatically release locked seats.

- **Seat Lock Expiry:** Seat locks automatically expire after the defined grace period (10 minutes). Expired locks must be released and made available for other users.

- **Idempotency:** Critical operations such as payment processing and booking creation must be idempotent to prevent duplicate transactions due to retries or network failures.

- **Booking Lifecycle:** Bookings transition through defined states: INITIATED → PAYMENT\_PENDING → CONFIRMED → CANCELLED → REFUNDED. State transitions must be atomic and consistent.


## **6.2 Auditing and Reliability**

- **Audit Logging:** All critical actions (booking creation, cancellation, payment updates, admin approvals) must be logged with timestamp and actor identity.

- **Failure Handling:** The system must gracefully handle failures such as payment gateway errors, email delivery failures, and concurrent booking conflicts without data inconsistency.

- **Data Retention:** Critical entities such as bookings and payments must not be permanently deleted. Instead, logical deletion or status-based lifecycle management should be used. Historical data must be retained for audit purposes.

- **Authorization:** All API endpoints must enforce role-based access control to ensure users can only access resources permitted to their role.


# **7. Approvals**

[`Davish E`](mailto:davish.std@gmail.com)\
**Lead Architect**

[`Davish E`](mailto:davish.std@gmail.com)\
**Product Owner**

****
