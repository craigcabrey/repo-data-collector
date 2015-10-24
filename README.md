# repo-data-collector

Simple program that uses a thread pool to collect data about
repositories from the GitHub API. It does this using a simple
`GET` request for the following URL:

`https://api.github.com/repos/:owner/:repository`

## Data Source

Included is a sample CSV with a list of 1,000 repositories with
which to test out the system.

Additionally, the system requires a GitHub OAuth access token to
authenticate with the GitHub API in order to complete the request
for the 1,000 repositories (unauthenticated requests are limited
to 60 requests per hour whereas authenticated requests are
5,000 per hour per user).

Generating an access token can be done by going to the following
url:

`https://github.com/settings/tokens`

and selecting "Generate new token". Paste this token into the
program when prompted to do so.

## Results

The program does not parse the resulting JSON data returned by the
API. Rather, it simply dumps it out to the console for demonstration
purposes.

## Running

`./repo-data-collector.exe repos.csv`