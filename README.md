# Modern Link Shortener

This project is a scalable and feature-rich **Link Shortener** built with modern technologies in .NET 8. It employs Clean Architecture principles to ensure maintainability, testability, and scalability. The application consists of an **ASP.NET Core Web API** as the backend and a **Blazor WebAssembly** front-end. It leverages a wide range of technologies and design patterns to offer a secure, efficient, and user-friendly experience.

## 🚀 Key Features

- **Secure Link Shortening**: Utilizes **SHA-256** hashing with collision management
- **Custom Link Support**: Create personalized short Links (Premium Feature)
- **Monetization**: Generates Revenue By Advertisement.
- **Supporting Both Monetized Redirection and Plain Redirection**
- **Real-time Statistics**: Live tracking and updates via **SignalR**
- **Batch Processing**: Support for bulk Link shortening operations
- **Advanced Analytics**: Detailed insights including geographical info and device info based on IP Address and User Agent.
- **Secure Authentication**: JWT-based auth with XSS & CSRF protection.
- **Rate Limiting**: Intelligent request throttling based on endpoint sensitivity
- **Link Management** With One Click:
  - Toggle Link activation.
  - Toggle Link **Monetization**.
  - Custom collision handling
  - Non-numeric hash guarantees

## 🛡 Security Features

- **HTTP-only cookie**-based JWT and refresh token storage.
- **CSRF protection** via token forgery prevention.
- Password hashing using **BCrypt**.
- **Rate limiting** mechanisms.
- **XSS attack prevention** measures.
- **Cloudflare Turnstile** integration for pages to prevent bots.
- IP and user agent processing using external trusted services.
- **User Secrets** Usage of User Secrets to prevent leakage of API Keys or other vital information.
- **Role-Based Authorization**
- **Strict Type-Safe Data Validation**

## 💻 Technical Stack

### Backend (.NET 8)
- **Architecture**: Clean Architecture
- **API**: ASP.NET Core Web API
- **Authentication**: JWT with secure cookie storage
- **Database**: PostgreSQL (Dockerized)
- **Caching & Message Broker**: Redis (Dockerized)
- **ORM**: Entity Framework Core
- **Real-time Communication**: SignalR
- **Payment Processing**: ZibalClient

### Frontend (.NET 8)
- **Technology**: Blazor WebAssembly
- **CSS Framework**: Bootstrap
- **Authentication** Using Custom HTTP Handler to authenticate requests
- **Authorization**: Using .NET Authorization State Provider
- **Real-time Communication**: SignalR Client
- **LocalStorage and Notification**: Blazored
- **Icons**: Blazorise Font Awesome Icons
- **Captcha**: Blazor Turnstile


### Key Packages
- **SignalR**: for Real-time communication
- **AutoMapper** for DTO mappings
- **FluentValidation** for request validation
- **IPinfo** for IP-based analytics
- **Npgsql** for PostgreSQL connectivity
- **BCrypt.NET** for password security
- **Chart.js** for showing Charts in Dashboard
- **Blazored**: for localstorage and showing notifications
- **Blazorise**: for ui elements like icons


## 🔧 Infrastructure

- Containerized PostgreSQL database
- Containerized Redis for caching and message brokering
- Background services for async operations
- **Email service** for authentication codes

## 🌟 Advanced Features
### Monetization
- **Advertisement**: support for earning money by showing ads in redirection page by up to 8 ads.

### Data Management
- Custom pagination implementation
- Transaction management for Link operations to ensure ***Atomicity***
- Collision handling strategy
- Non-numeric hash generation

### Caching System
- Generic Redis caching implementation
- Batch caching support
- Optimized performance for high-traffic scenarios
- Redis queue for background service communication

### Analytics
- Detailed visitor analytics
- Device and browser detection
- Geographic location tracking
- Background processing of IP and user agent data
- Comprehensive user dashboard with customer insights:
    - Chart showing Past Month's Clicks.
    - Chart Showing in what time of day are the clicks more likely to happen.
    - Weekly Click Growth
    - Total Links Count
    - Clicks Yesterday
    - Average Clicks Per Link
    - Top Clicked Links list.
    - Top Countries the clickers are from.
    - Top devices clickers use.
    - Table showing the 5 most recent shortened Links.

### Link Shortening
- Support for both Single and Batch Link Shortening
- Toggles For Both Activation and Monetization of Shortened Links
- Easy-to-use and custom Tables to manage shortened links in user profile and shortener page.

### Real-time Updates
- SignalR integration for live statistics 
- Instant balance and profile stats updates
- Real-time user profile synchronization
