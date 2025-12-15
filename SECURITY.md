# Security Policy

## Supported Versions

We currently support the following versions of LogRotate for Windows with security updates:

| Version | Supported          |
| ------- | ------------------ |
| 0.0.x   | :white_check_mark: |
| < 0.0.20| :x:                |

## Reporting a Vulnerability

We take the security of LogRotate for Windows seriously. If you discover a security vulnerability, please follow these steps:

### How to Report

**Please do NOT report security vulnerabilities through public GitHub issues.**

Instead, please report them via one of these methods:

1. **GitHub Security Advisories** (Preferred)
   - Navigate to the [Security tab](https://github.com/ken-salter/logrotatewin/security) of this repository
   - Click "Report a vulnerability"
   - Fill out the advisory form with details

2. **Direct Contact**
   - Contact the maintainers directly through GitHub
   - Create a private security advisory

### What to Include

Please include as much of the following information as possible:

- Type of vulnerability (e.g., buffer overflow, injection, privilege escalation)
- Full paths of source file(s) related to the vulnerability
- Location of the affected source code (tag/branch/commit or direct URL)
- Step-by-step instructions to reproduce the issue
- Proof-of-concept or exploit code (if available)
- Impact of the vulnerability, including how an attacker might exploit it

### What to Expect

- **Acknowledgment**: We will acknowledge receipt of your vulnerability report within 48 hours
- **Assessment**: We will assess the vulnerability and determine its severity within 5 business days
- **Updates**: We will keep you informed of our progress toward fixing the vulnerability
- **Resolution**:
  - **Accepted vulnerabilities**: We will work on a fix and release a patched version. You will be credited in the release notes (unless you prefer to remain anonymous)
  - **Declined vulnerabilities**: We will explain why we don't consider the issue a security vulnerability

### Disclosure Policy

- Please allow us reasonable time to address the vulnerability before public disclosure
- We aim to release security patches within 30 days of confirmation for critical vulnerabilities
- We will coordinate with you on the disclosure timeline
- Once a fix is released, we will publish a security advisory on GitHub

## Security Update Process

When a security vulnerability is fixed:

1. A new version is released with the fix
2. A security advisory is published on GitHub
3. The vulnerability is documented in the [Release Notes](logrotate/Content/README.md#release-notes)
4. Users are notified through GitHub releases and advisories

## Security Best Practices

When using LogRotate for Windows:

- **Run with least privilege**: Use a dedicated service account with minimal required permissions
- **Restrict configuration files**: Ensure `.conf` files are only writable by administrators
- **Validate file paths**: Be cautious with file paths in configuration to prevent path traversal
- **Monitor logs**: Review rotation logs for unexpected behavior
- **Keep updated**: Always use the latest version to benefit from security fixes
- **Secure scripts**: If using `prerotate`/`postrotate` scripts, ensure they are secure and from trusted sources

## Known Security Considerations

- **File System Access**: LogRotate for Windows requires appropriate file system permissions to read, rotate, and compress log files
- **Script Execution**: When using `prerotate` or `postrotate` directives, ensure scripts are from trusted sources as they run with the application's privileges
- **Configuration Files**: Configuration files (`.conf`) should be protected from unauthorized modification as they control file operations

## Vulnerability Disclosure History

No security vulnerabilities have been publicly disclosed for this project at this time.

## Questions?

If you have questions about this security policy or the security of LogRotate for Windows, please open a public GitHub issue (for non-sensitive questions) or contact the maintainers directly.

## Additional Resources

- [GitHub Security Advisories Documentation](https://docs.github.com/en/code-security/security-advisories)
- [Common Vulnerabilities and Exposures (CVE)](https://cve.mitre.org/)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
