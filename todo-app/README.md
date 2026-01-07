# Angular Todo App

A focused Todo app built with Angular. It supports adding tasks, marking them complete, and clearing finished items.

## Prerequisites

- Node.js 16.x
- npm 8+ (comes with Node.js)
- Angular CLI (optional): `npm install -g @angular/cli@15`

## Setup

Install dependencies:

```bash
npm install
```

## Run the app

```bash
ng serve
```

Open `http://localhost:4200`.

If you do not have the CLI installed globally, use:

```bash
npx ng serve
```

## Backend API

The UI calls the .NET backend in `../BE`.

Default API base URL: `http://localhost:5148`

You can change it in `src/environments/environment.ts`.

## Build for production

```bash
ng build --configuration production
```
