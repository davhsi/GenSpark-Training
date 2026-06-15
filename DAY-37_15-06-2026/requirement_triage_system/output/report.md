# Client Requirement Analysis Report

**Source:** Project Requirement - Reg
**Generated:** 2026-06-15 15:55:32 UTC

## Functional Requirements
- **id**: FR-001 | **title**: Employee login via SSO | **description**: Employees must authenticate using the company's existing single sign‑on system to access the application | **priority**: High | **acceptance_criteria**: User can successfully log in with corporate credentials and is redirected to the dashboard
- **id**: FR-002 | **title**: View remaining vacation days | **description**: After login, employees can see their current vacation day balance | **priority**: High | **acceptance_criteria**: Dashboard displays accurate remaining vacation days for the logged‑in employee
- **id**: FR-003 | **title**: Submit leave request | **description**: Employees can create a new leave request by specifying start and end dates | **priority**: High | **acceptance_criteria**: System records a leave request with the entered dates and shows it in the employee's request list
- **id**: FR-004 | **title**: Manager view team requests | **description**: Managers have a screen that lists all leave requests submitted by members of their team | **priority**: High | **acceptance_criteria**: Manager can see a table of pending, approved, and denied requests for all direct reports
- **id**: FR-005 | **title**: Approve or deny request | **description**: Managers can approve or deny a leave request with a single action | **priority**: High | **acceptance_criteria**: Manager action updates request status to Approved or Denied and triggers notification workflow
- **id**: FR-006 | **title**: Email notification on decision | **description**: When a manager approves or denies a request, the employee receives an email informing them of the decision | **priority**: High | **acceptance_criteria**: Employee receives an email within 5 minutes of the manager's decision containing request details and outcome
- **id**: FR-007 | **title**: Responsive mobile‑friendly UI | **description**: The web application must be usable on mobile devices with a responsive layout | **priority**: Medium | **acceptance_criteria**: All screens render correctly on common smartphone screen sizes without horizontal scrolling
- **id**: FR-008 | **title**: Fast application performance | **description**: The application should respond quickly to user actions | **priority**: Medium | **acceptance_criteria**: Page load and key interactions complete within 2 seconds under typical load

## Non-Functional Requirements
- **id**: NFR-001 | **category**: Performance | **description**: The system must provide quick response times for all user interactions | **metric**: < 2 seconds page load and action latency for 95% of requests
- **id**: NFR-002 | **category**: Usability | **description**: The UI must be intuitive and accessible on mobile devices | **metric**: Responsive design passing Google Lighthouse mobile usability score ≥ 90
- **id**: NFR-003 | **category**: Security | **description**: Authentication must use the corporate single sign‑on solution and enforce role‑based access | **metric**: Integration with SSO using SAML/OIDC; only employees and managers can access respective screens
- **id**: NFR-004 | **category**: Reliability | **description**: Email notifications must be reliably delivered after a decision is made | **metric**: ≥ 99.5% delivery success rate, retries for failed sends up to 3 times

## Risks
- **id**: RISK-001 | **description**: Integration with the existing corporate SSO may encounter compatibility or configuration issues | **impact**: High | **likelihood**: Medium | **mitigation**: Engage security team early, obtain SSO documentation, prototype authentication flow
- **id**: RISK-002 | **description**: Email delivery failures could leave employees uninformed of decisions | **impact**: High | **likelihood**: Medium | **mitigation**: Use a proven email service with retry logic and monitor delivery logs
- **id**: RISK-003 | **description**: Performance expectations are vague, risking unmet user expectations | **impact**: Medium | **likelihood**: High | **mitigation**: Clarify latency targets with client and conduct load testing during development
- **id**: RISK-004 | **description**: Source of vacation balance data is unspecified, risking inaccurate displays | **impact**: Medium | **likelihood**: High | **mitigation**: Confirm data source (HR system/API) and define integration contract early

## Assumptions
- **id**: ASMP-001 | **description**: The corporate SSO provides authenticated user identity and role information (employee vs manager)
- **id**: ASMP-002 | **description**: Vacation balance is stored in an existing HR system that can be accessed via an API or database view
- **id**: ASMP-003 | **description**: An email service (SMTP server or third‑party provider) is available for sending notifications
- **id**: ASMP-004 | **description**: Manager‑employee relationships are defined within the SSO directory or a related HR service

## Questions for Client
- **id**: Q-001 | **question**: Which SSO protocol and provider are used (e.g., SAML 2.0, OpenID Connect, OAuth2)? | **context**: Needed to design authentication integration and estimate effort
- **id**: Q-002 | **question**: Where is the employee vacation balance stored and how can it be accessed? | **context**: Required to display accurate remaining days to users
- **id**: Q-003 | **question**: What specific performance targets are expected (e.g., page load ≤ 2 s, API response ≤ 200 ms)? | **context**: Clarifies the “fast” requirement for sizing and testing
- **id**: Q-004 | **question**: Which email service or infrastructure should be used for notifications, and are there template requirements? | **context**: Ensures reliable delivery and consistent communication format
- **id**: Q-005 | **question**: Are there any additional approval workflow rules (e.g., escalation, multi‑level approval) beyond a single manager decision? | **context**: Impacts the design of the approval process and data model
- **id**: Q-006 | **question**: Should employees be able to cancel or modify pending leave requests? | **context**: Affects request lifecycle handling and UI design
- **id**: Q-007 | **question**: Are there audit logging or compliance requirements for leave request actions? | **context**: Determines need for logging, retention, and security controls
