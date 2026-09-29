import { cva } from 'class-variance-authority';

// shadcn/ui button styles. Every size keeps the 44px minimum touch target.
export const buttonVariants = cva(
  "inline-flex min-h-touch shrink-0 items-center justify-center gap-2 rounded-md font-semibold whitespace-nowrap transition-colors disabled:pointer-events-none disabled:opacity-50 [&_svg]:pointer-events-none [&_svg:not([class*='size-'])]:size-5 [&_svg]:shrink-0",
  {
    variants: {
      variant: {
        default: 'bg-primary text-primary-foreground hover:bg-primary/90',
        secondary: 'bg-secondary text-secondary-foreground hover:bg-secondary/80',
        outline: 'border-(length:--border-width) bg-card hover:bg-accent hover:text-accent-foreground',
        ghost:
          'hover:bg-accent hover:text-accent-foreground aria-pressed:bg-accent aria-pressed:text-accent-foreground aria-pressed:ring-(length:--border-width) aria-pressed:ring-foreground',
        destructive: 'bg-destructive text-destructive-foreground hover:bg-destructive/90',
      },
      size: {
        default: 'px-4 py-2 text-base',
        lg: 'px-6 py-3 text-lg',
        touch: 'min-w-touch px-2.5',
      },
    },
    defaultVariants: { variant: 'default', size: 'default' },
  },
);
