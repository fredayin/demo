package org.sasanlabs.vulnerabilities.fileupload.service;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import org.apache.commons.fileupload.FileItem;
import org.sasanlabs.framework.VulnerableAppException;
import org.sasanlabs.framework.VulnerableAppUtility;
import org.sasanlabs.framework.i18n.Messages;
import org.sasanlabs.vulnerableapp.facade.schema.Variant;
import org.sasanlabs.vulnerableapp.facade.schema.VulnerabilityLevelDefinition;
import org.sasanlabs.vulnerableapp.facade.schema.VulnerabilityLevelHint;
import org.sasanlabs.vulnerableapp.facade.schema.VulnerabilityType;

/** @author preetkaran20@gmail.com KSASAN */
public class FileUploadLevel2 extends AbstractFileUpload {

    private static final String LEVEL = "LEVEL_2";
    private static final List<String> UNALLOWED_EXTENSIONS =
            Arrays.asList("html", "htm", "svg", "dhtml", "shtml", "xhtml", "xml", "svgz");

    @Override
    public VulnerabilityLevelDefinition getVulnerabilityLevelDefinition() {
        List<VulnerabilityLevelHint> hints = new ArrayList<VulnerabilityLevelHint>();
        hints.add(
                new VulnerabilityLevelHint(
                        Collections.singletonList(
                                new VulnerabilityType("Custom", "UnrestrictedFileUpload")),
                        Messages.getMessage("FILE_UPLOAD_IF_NOT_HTML_FILE_EXTENSION")));
        return AbstractFileUpload.getFileUploadVulnerabilityLevelDefinition(
                LEVEL, "", Variant.UNSECURE, hints);
    }

    @Override
    public boolean validate(FileItem fileItem) throws VulnerableAppException {
        String extension = VulnerableAppUtility.getExtension(fileItem.getName());
        if (extension == null) {
            return false;
        }
        return !UNALLOWED_EXTENSIONS.contains(extension);
    }
}
