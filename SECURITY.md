# Security Policy

## Reporting a Vulnerability

If you discover a security vulnerability in the FBI Form 2257 System, please report it responsibly to:

**Email**: security@yourdomain.com

Please include:
1. Description of the vulnerability
2. Steps to reproduce the issue
3. Potential impact
4. Suggested fix (if available)

**Do NOT** open a public GitHub issue for security vulnerabilities.

## Security Considerations

### For Operators

This application processes sensitive personal information including:
- Full legal names
- Dates of birth
- Address information
- Identity document images

**Your responsibilities:**

1. **Data Protection**
   - Ensure TLS/SSL encryption in transit
   - Restrict access to storage directories
   - Implement proper file permissions
   - Regular backups with access controls

2. **Access Control**
   - Use strong authentication for administrative access
   - Limit network access to the application
   - Use firewall rules to restrict connections
   - Monitor access logs regularly

3. **Compliance**
   - Comply with all applicable data protection laws (GDPR, CCPA, etc.)
   - Maintain audit logs
   - Implement incident response procedures
   - Have data breach notification plan

4. **Maintenance**
   - Keep .NET runtime updated
   - Apply security patches promptly
   - Monitor dependencies for vulnerabilities
   - Regular security audits

### For Users

1. **Producer Verification**
   - Verify performer identity using additional means beyond this form
   - Keep all records in a secure location
   - Follow all federal and local regulations

2. **Privacy**
   - Only share form URLs with authorized performers
   - Do not reuse UUIDs
   - Secure all copies of completed forms

3. **Compliance**
   - Ensure all information is accurate and truthful
   - Maintain records per 18 U.S.C. § 2257 requirements
   - Retain records for appropriate periods
   - Be prepared for Department of Justice inspections

## Security Features

The application includes:

- **Rate Limiting**: Prevents abuse and DDoS attacks
- **Input Validation**: All inputs validated before processing
- **File Validation**: File type and size checking
- **Age Verification**: Enforces 18+ requirement
- **Secure UUIDs**: Cryptographically secure random identifiers
- **Logging**: Comprehensive audit trail
- **Error Handling**: Secure error messages (no sensitive data exposure)
- **Container Security**: Non-root user execution in Docker

## Known Limitations

- **File Storage**: Uses local filesystem; consider S3 for distributed systems
- **Database**: Not included; add if additional persistence needed
- **Authentication**: Public by design; add authentication layer if needed
- **Encryption**: Data at rest not encrypted; implement if required
- **API Keys**: No API key management; add if needed

## Security Best Practices

### Deployment

```bash
# Use environment variables for sensitive config
export Storage__BasePath=/secure/storage/path

# Run with least privileges
docker run --user 1001 form2257:latest

# Enable security scanning in CI/CD
docker scan form2257:latest

# Keep container base image updated
docker pull mcr.microsoft.com/dotnet/aspnet:9.0
```

### Monitoring

```bash
# Monitor for suspicious activity
tail -f logs/app-*.txt | grep ERROR

# Check for rate limit violations
grep "rate limit exceeded" logs/app-*.txt

# Monitor disk usage
df -h /app/data
```

### Updates

1. Subscribe to security advisories
2. Monitor NuGet package updates
3. Test updates in staging first
4. Apply patches promptly in production
5. Maintain audit trail of updates

## Vulnerability Disclosure Timeline

- **Day 0**: Vulnerability reported
- **Day 1**: Initial assessment and acknowledgment
- **Day 7**: Patch or mitigation plan provided
- **Day 30**: Public disclosure (if patch available)
- **Day 90**: Public disclosure (if still unpatched)

## Compliance

This application helps with compliance but does not guarantee it. Users are responsible for:

- 18 U.S.C. § 2257 - Record keeping requirements
- State and local regulations
- Age verification laws
- Data protection regulations (GDPR, CCPA, etc.)
- Record retention requirements

## Third-Party Vulnerabilities

### Dependencies

The application uses:
- **QuestPDF**: For PDF generation
- **Serilog**: For logging
- **.NET Runtime**: Core framework

Monitor these for security updates using:

```bash
# Check for vulnerabilities
dotnet package search --vulnerable

# Update packages
dotnet package update
```

## Security Scanning

Enable in CI/CD:

```yaml
# GitHub Actions
- name: Security Scanning
  run: |
    dotnet add package SecurityCodeScan
    dotnet build /p:TreatWarningsAsErrors=true
```

## Questions?

For security-related questions, email: security@yourdomain.com

For general security questions, see: [CONTRIBUTING.md](CONTRIBUTING.md)

---

**Last Updated**: April 24, 2026  
**Version**: 1.0.0
