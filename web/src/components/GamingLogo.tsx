type GamingLogoProps = {
  className?: string
}

export const GamingLogo = ({ className }: GamingLogoProps) => (
  <svg
    className={className}
    viewBox="0 0 80 40"
    fill="none"
    xmlns="http://www.w3.org/2000/svg"
    role="img"
    aria-label="HYPER RACING logo"
  >
    <defs>
      <linearGradient id="logoStroke" x1="0" y1="0" x2="80" y2="40" gradientUnits="userSpaceOnUse">
        <stop stopColor="#ff2738" />
        <stop offset="0.6" stopColor="#ff6b6b" />
        <stop offset="1" stopColor="#ffffff" />
      </linearGradient>
    </defs>
    <rect x="3" y="6" width="74" height="28" rx="14" fill="rgba(255,255,255,0.04)" stroke="url(#logoStroke)" />
    <path
      d="M15 27V13h6.4l3 5.6L27.4 13H34v14h-4.5V20.4l-3.5 5.4h-2L20.4 20v7H15Zm28.2 0-6-14h4.8l3.2 8.4L48.6 13h4.6l-6 14h-4Zm13.6 0V13h11.2c2.6 0 4.2 1.5 4.2 3.6 0 1.4-.7 2.7-2.1 3.2 1.7.4 2.6 1.6 2.6 3.3 0 2.4-1.8 3.9-4.4 3.9H56.8Zm4.6-8.7h4.9c.9 0 1.4-.5 1.4-1.3 0-.8-.5-1.2-1.4-1.2h-4.9v2.5Zm0 5.2h5.2c.9 0 1.4-.5 1.4-1.3 0-.8-.5-1.3-1.4-1.3h-5.2v2.6Z"
      fill="url(#logoStroke)"
    />
  </svg>
)
