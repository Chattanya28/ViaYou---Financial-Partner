# ViaYou

ViaYou is a comprehensive, full-stack financial management platform built with ASP.NET Core 10 MVC that serves as a unified command center for personal finances. The application addresses the critical problem of fragmented financial management by aggregating multiple bank accounts, comparing investment opportunities across 15+ institutions, optimizing idle cash through intelligent detection, simulating financial decisions through interactive what-if scenarios, providing real-time security monitoring with actionable insights, and offering personalized investment recommendations—all within a single, beautifully designed dark-themed interface with 3D animations, glass morphism effects, and responsive design that works flawlessly across desktop, tablet, and mobile devices.

## Features

### Secure Authentication & Dashboard
- **Registration & Login**: Secure authentication system (register with full name, email, strict password requirements). Optional two-factor authentication, "remember me" functionality, rate limiting (locking accounts after 5 failed attempts).
- **Comprehensive Tracking**: Complete login history tracking (IP addresses, device information, geolocation data, timestamps).
- **Personalized Dashboard**: Time-based greetings and a real-time total net worth balance card with percentage change tracking.
- **Security Score Card**: Dynamic score (0-100) calculated from multiple security factors with a color-coded visual progress bar.
- **Quick Actions & Analytics**: Fast-access transaction buttons with 3D hover effects. Interactive spending charts (Chart.js) for daily, weekly, and monthly views, plus latest transactions and categorized spending analytics.

### Intelligent Savings & FD Engine
- **Idle Cash Detection**: Automatically identifies idle cash in savings accounts (balances > ₹25,000) and calculates potential extra earnings from fixed deposits.
- **FD Comparison Engine**: Comprehensive rate comparison across 15 major Indian banks (HDFC, ICICI, SBI, Axis, AU Small Finance, etc.). Calculates returns for amounts from ₹1,000 to ₹10,00,000 with highlighted "BEST" rates and additional earning breakdowns.

### Financial Goal Simulator
- **Interactive Simulator**: Transform financial planning (emergency funds, vacations, new car, etc.) into actionable strategies showing current progress and monthly contributions.
- **What-If Scenarios**: Adjust a slider from ₹1,000 to ₹1,00,000 to see the real-time mathematical impact of redirecting idle cash towards a goal. See extra interest earned, months saved, and an automatically adjusted target completion date.

### Security Hub
- **Security Management Center**: Take control with toggles for 2FA, biometric login, transaction confirmation, and login alerts.
- **Alerts & Devices**: Active alerts for password expiration and new device logins. Trusted devices list with one-click revocation.
- **Security Checklist**: Gamified checklist for best practices, directly impacting the account's security score.

## Architecture & Tech Stack

- **Framework**: ASP.NET Core 10 MVC
- **Database**: Entity Framework Core with SQLite
- **Projects**: Clean separation of concerns with `ViaYou.Core` (entities/interfaces), `ViaYou.Web` (controllers/views), and `ViaYou.Tests` (unit testing).
- **Identity**: Robust ASP.NET Core Identity setup for user and role management.
- **Frontend**: Tailwind CSS, Font Awesome 6, Chart.js, and custom responsive UI/UX with modern glass-morphism and animation effects.

## Data Structure & Isolation

The system supports full CRUD operations on individual accounts, transactions, goals, and security preferences. Complete user-specific data isolation ensures individual privacy. 
New registrations automatically receive an initial set of demonstration data, including two bank accounts ("Demo Bank" and "Demo Salary") and one goal ("Emergency Fund").

## Future Roadmap

Built with extensibility in mind, future expansion options include:
- Real banking API integrations via the Account Aggregator framework.
- Inclusion of Mutual Funds, Gold Products, Stocks, and Insurance options.
- Machine Learning models for automatic transaction categorization and intelligent insights.
- Advanced infrastructure implementations like Docker containers, CI/CD with GitHub Actions, and cloud hosting on Azure or AWS.

## Demo

All features are fully functional and ready for demonstration. 

Use the following credentials to experience the unified financial command center:
- **Email:** `demo@viayou.com`
- **Password:** `Demo@123`
