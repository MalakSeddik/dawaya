import { LogIn } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router';

import { Button } from '@/components/ui/button';

interface SignInPromptProps {
  message: string;
}

// Guests can browse everything (BR-04); saving and family profiles need an account (BR-05).
export function SignInPrompt({ message }: SignInPromptProps) {
  const { t } = useTranslation();
  return (
    <div className="flex flex-col items-center gap-4 rounded-lg border-(length:--border-width) bg-card p-6 text-center">
      <LogIn aria-hidden="true" className="size-10 text-primary" />
      <p className="text-lg">{message}</p>
      <div className="flex flex-wrap justify-center gap-2">
        <Button asChild>
          <Link to="/login">{t('auth.signIn')}</Link>
        </Button>
        <Button asChild variant="outline">
          <Link to="/register">{t('auth.createAccount')}</Link>
        </Button>
      </div>
    </div>
  );
}
