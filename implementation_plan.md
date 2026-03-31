# Implement ViaYou Missing Modules (Compare FD & Security Hub)

Based on the prompt and the existing architecture (including the dark-theme UI with 3D elements in `_Layout.cshtml` and `Dashboard/Index.cshtml`), we need to build the missing core features of the ViaYou platform. 

## Features to Implement

### 1. FD Comparison Engine (`CompareController` & `Compare/Index.cshtml`)
- **Controller Logic**: 
  - Create `CompareController.cs` in the Web project.
  - Fetch dummy/seeded fixed deposit rates for the 15+ banks requested (AU Small Finance, Equitas, HDFC, ICICI, etc.) from `BankPolicy` entities or initialize them if missing.
  - Render an initial view with the rates sorted by highest interest.
- **UI & Layout**:
  - Implement an interactive slider or number input allowing users to select an investment amount (₹1,000 to ₹10,00,000).
  - Use JavaScript to calculate returns dynamically in real-time on the client side without full page reloads.
  - Create sleek comparison cards utilizing existing classes (`bg-dark-card`, `card-3d`, `border-dark-border`).
  - Implement a distinctive green background and "BEST" badge for the top bank.
  - Compare earnings against a standard 3.5% savings account and highlight the extra amount earned.

### 2. Security Hub (`SecurityController` & `Security/Index.cshtml`)
- **Controller Logic**:
  - Create `SecurityController.cs` in the Web project.
  - Fetch basic `User` security settings and mock data for recent logins (from `LoginHistory`), active sessions, and trusted devices.
- **UI & Layout**:
  - Implement a large Security Score card (0-100) with a gradient progress bar.
  - Add realistic CSS toggle switches (using standard checkbox hack or JS toggles) for 2FA, Biometric Login, Login Alerts, and Transaction Confirmation with hover animations and notifications.
  - Create list items for Active Alerts (e.g., password expiring) and Trusted Devices (with an interactive "Revoke" button).
  - Implement an interactive Security Checklist that increases the score visually when clicked.

## User Review Required

> [!IMPORTANT]  
> Since you have already established a complex visual language (glass morphism, gradients, hover states) in `_Layout.cshtml` and `Dashboard`, I will exactly follow your established Tailwind classes, custom CSS (`.card-3d`, `.btn-3d`), and layout structures.
> **Question**: Should the FD comparison engine calculate exact maturity amounts for purely 1-year terms by default, or should we include a tenure slider as well?

## Proposed Changes

### Controllers
#### [NEW] [CompareController.cs](file:///C:/Users/chatt/source/repos/ViaYou/ViaYou/Controllers/CompareController.cs)
#### [NEW] [SecurityController.cs](file:///C:/Users/chatt/source/repos/ViaYou/ViaYou/Controllers/SecurityController.cs)

### Views
#### [NEW] [Compare/Index.cshtml](file:///C:/Users/chatt/source/repos/ViaYou/ViaYou/Views/Compare/Index.cshtml)
#### [NEW] [Security/Index.cshtml](file:///C:/Users/chatt/source/repos/ViaYou/ViaYou/Views/Security/Index.cshtml)

## Verification Plan

### Manual Verification
1. Launch the application and click on **"Compare FD"** in the navigation bar. 
2. Verify that interacting with the FD amount input fluidly recalculates the returns for all 15 banks with smooth animations.
3. Click on **"Security"** in the navigation bar. Check that toggling security switches provides visual feedback and that the Security Score calculates correctly.
4. Verify responsiveness on small-screen sizes (collapsing correctly to the bottom nav bar you've already set up).
