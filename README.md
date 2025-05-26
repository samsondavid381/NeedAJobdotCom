# NeedAJobdotCom

NeedAJobdotCom is a simple job board-style web app built using ASP.NET Core. It follows a clean, modular structure to keep things organized and easy to extend. The goal is to provide a backend for handling job posts and applications, along with a lightweight front end.

## Project Structure

Here's a quick breakdown of the solution:

- **NeedAJobdotCom.Api** – Main Web API. Handles routing, controllers, and request/response logic.
- **NeedAJobdotCom.Core** – Domain layer. This is where the core business logic and interfaces live.
- **NeedAJobdotCom.Infrastructure** – Data access layer and other implementation details (e.g., database, services).
- **NeedAJobdotCom.Front** – Basic front-end UI (likely static files or a small JS app).

## Getting It Running

To get the backend up and running:

1. Clone the repo:

   ```bash
   git clone https://github.com/samsondavid381/NeedAJobdotCom.git
   cd NeedAJobdotCom
   ```

2. Restore dependencies:

   ```bash
   dotnet restore
   ```

3. Build everything:

   ```bash
   dotnet build
   ```

4. Start the API:

   ```bash
   cd NeedAJobdotCom.Api
   dotnet run
   ```

   You should see it running on `http://localhost:5000` (or `https://localhost:5001` depending on your setup).

5. Frontend:

   Go into the `NeedAJobdotCom.Front` folder and open `index.html` in a browser, or hook it up to your preferred frontend setup if you're planning to expand it.

## Stack

- ASP.NET Core for the API
- C# for the core logic
- Basic HTML/JS for the front end (can be swapped out with a full framework if needed)
- Clean-ish architecture to keep things maintainable

## Notes

The project is still a work in progress, so expect some gaps here and there. The structure is in place though, so it’s easy to plug in a DB, auth system, or whatever other features you need.

If you have questions or want to collaborate, just reach out.
