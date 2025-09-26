# Frontend Rule

Enforces frontend development standards for Next.js and React Native components in the Hotshot Logistics platform.

## When to Apply

Apply when developing the admin dashboard (Next.js), driver mobile app (React Native), or any frontend components that interact with the backend API.

## Next.js Admin Dashboard

### Component Architecture Patterns

#### Atomic Design Structure
- **Atoms**: Basic UI elements (buttons, inputs, icons) in `components/ui/`
- **Molecules**: Combinations of atoms (form fields, cards) in `components/ui/`
- **Organisms**: Complex components (data tables, forms) in `components/`
- **Templates**: Page layouts in `components/templates/`
- **Pages**: Route-specific components in `app/` (App Router)

#### Component Organization
- Use TypeScript for all components
- Implement proper prop interfaces with `interface ComponentProps`
- Export components as named exports
- Use default exports only for page components
- Place related components in feature folders: `components/features/{feature}/`

#### Component Best Practices
- Keep components small and focused (single responsibility)
- Use functional components with hooks
- Implement error boundaries for complex components
- Use React.memo for expensive components when appropriate
- Follow naming convention: PascalCase for components, camelCase for instances

### State Management Guidelines

#### Local State
- Use `useState` for simple component state
- Use `useReducer` for complex state logic with multiple actions
- Lift state up when shared between sibling components
- Use `useContext` sparingly, prefer prop drilling for clarity

#### Global State
- Use React Query (TanStack Query) for server state
- Implement optimistic updates for better UX
- Cache data appropriately based on update frequency
- Use React Query DevTools in development

#### State Management Rules
- Server state: React Query
- Client state: useState/useReducer
- Form state: React Hook Form with Zod validation
- UI state: useState or custom hooks
- Avoid Redux unless absolutely necessary

### API Integration Standards

#### Data Fetching
- Use React Query for all API calls
- Implement proper loading, error, and success states
- Use query keys for cache invalidation
- Implement retry logic with exponential backoff
- Prefetch data for better performance

#### API Client
- Create typed API client functions in `lib/api/`
- Use fetch with proper error handling
- Implement request/response interceptors if needed
- Validate API responses with Zod schemas
- Handle authentication tokens automatically

#### Error Handling
- Display user-friendly error messages
- Implement retry mechanisms for failed requests
- Log errors to monitoring service
- Show loading states during API calls
- Handle network errors gracefully

## React Native Mobile App

### Cross-Platform Development Practices

#### Platform-Specific Code
- Use `Platform.select()` for platform differences
- Create platform-specific files: `Component.ios.tsx`, `Component.android.tsx`
- Minimize platform-specific code (< 10% of codebase)
- Test on both platforms regularly

#### Navigation
- Use Expo Router for file-based routing
- Implement deep linking support
- Handle back button behavior properly
- Use navigation guards for protected routes

#### Device Features
- Request permissions appropriately
- Handle app state changes (foreground/background)
- Implement proper background task handling
- Use device sensors responsibly

### Mobile-Specific UI Patterns

#### Responsive Design
- Use flexbox for layout (avoid absolute positioning)
- Implement responsive breakpoints
- Handle different screen sizes and orientations
- Use SafeAreaView for notch devices

#### Touch Interactions
- Implement proper touch targets (44pt minimum)
- Use appropriate feedback (haptic, visual)
- Handle long press and swipe gestures
- Implement pull-to-refresh patterns

#### Performance Patterns
- Use FlatList with proper keyExtractor
- Implement virtualization for large lists
- Use Image component with proper sizing
- Optimize animations with Animated API

### Performance Optimization Techniques

#### Code Splitting
- Use dynamic imports for route-based splitting
- Implement lazy loading for heavy components
- Split vendor chunks from app code
- Use React.lazy for component code splitting

#### Image Optimization
- Use appropriate image formats (WebP when supported)
- Implement lazy loading for images
- Compress images and use responsive images
- Cache images appropriately

#### Bundle Optimization
- Analyze bundle size with build tools
- Remove unused dependencies
- Use tree shaking effectively
- Implement proper chunk splitting

#### Memory Management
- Clean up event listeners and timers
- Unsubscribe from subscriptions on unmount
- Use useCallback and useMemo appropriately
- Monitor memory usage in development

## Code Quality

### TypeScript Standards

#### Type Safety
- Use strict TypeScript configuration
- Avoid `any` type; use proper type definitions
- Implement proper generic constraints
- Use utility types (Partial, Pick, Omit) appropriately

#### Interface Design
- Define interfaces for all data structures
- Use discriminated unions for variant types
- Implement proper error types
- Export types from central locations

#### Type Organization
- Place types in `types/` directory
- Use barrel exports for clean imports
- Generate types from API schemas when possible
- Document complex types with JSDoc

### Component Testing Requirements

#### Unit Testing
- Test all custom hooks with React Testing Library
- Mock API calls in component tests
- Test user interactions and state changes
- Achieve 70%+ coverage for components

#### Integration Testing
- Test component integration with React Query
- Validate form submissions and validation
- Test navigation flows
- Use test IDs for reliable element selection

#### E2E Testing
- Test critical user journeys
- Validate API integration end-to-end
- Test on real devices when possible
- Include accessibility testing

### Accessibility Compliance

#### WCAG Guidelines
- Implement proper heading hierarchy
- Use semantic HTML elements
- Provide alt text for images
- Ensure sufficient color contrast

#### Mobile Accessibility
- Support screen readers (VoiceOver, TalkBack)
- Implement proper focus management
- Use large touch targets
- Support dynamic text sizing

#### Testing Accessibility
- Use axe-core for automated testing
- Manual testing with assistive technologies
- Include accessibility in code reviews
- Document accessibility features

## Development Workflow

### Code Organization
- Use feature-based folder structure
- Implement barrel exports for clean imports
- Follow consistent naming conventions
- Use ESLint and Prettier for code formatting

### Build and Deployment
- Use Next.js build optimization
- Implement proper environment variables
- Configure CI/CD for automated testing
- Use Expo EAS Build for mobile deployments

### Monitoring and Analytics
- Implement error tracking (Sentry)
- Add performance monitoring
- Track user analytics appropriately
- Monitor app store ratings and reviews

## References

See 6-Docs/frontend-examples.md for detailed implementation examples and component patterns.
See [Testing Rules](testing.md) for component testing standards.
See [Code Quality Rules](code-quality.md) for additional quality guidelines.