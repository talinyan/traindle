interface Update {
  date: string; // YYYY-MM-DD
  changes: { title: string; items: string[] }[];
}

// newest first
export const updates: Update[] = [
  {
    date: '2026-10-18',
    changes: [
      {
        title: 'Add country/continent info',
        items: ['Added country and continent info to system and line guesses']
      }
    ]
  },
  {
    date: '2026-10-15',
    changes: [
      {
        title: 'Added Endless Mode',
        items: [
          'Added "Endless" mode - play as many rounds as you like!',
          'Modified home page and reworked overall layout'
        ]
      }
    ]
  },
  {
    date: '2026-10-13',
    changes: [
      {
        title: 'Cleaner line names',
        items: [
          'Letter-only lines now show as "A Line" instead of "A"',
          'Guess autocomplete now loads all options and scrolls instead of being capped to 10 rows'
        ]
      }
    ]
  },
  {
    date: '2026-10-12',
    changes: [
      {
        title: 'Cleaner line names',
        items: [
          'Number-only lines now show as "Line 5" instead of "5"',
          'Line names no longer include years of operation, e.g. (2015-2017)'
        ]
      },
      {
        title: 'Altered compass direction',
        items: ['Direction hints now snap to the nearest 45° compass point (N, NE, E…) instead of an exact angle']
      }
    ]
  }
];
